using System.Runtime.InteropServices;
using System.Text;
using TerraFX.Interop.Windows;

namespace IIS.NativeAOT;

internal class CLRHost
{
    private static readonly CLRHost s_instance = new();

    unsafe class CallbackState
    {
        public delegate* unmanaged<IntPtr, IntPtr, IntPtr, int> RequestCallback = &ErrorPage;
        public delegate* unmanaged<IntPtr, IntPtr, uint, int, IntPtr, IntPtr, int> AsyncCallback;
        public IntPtr Context;
    }

    private bool _initialized;
    private int _returnCode;
    private byte[] _error =
            """
            <html>
               <head>
                <title>Internal Server Error</title>
               </head>
               <body>
               <h1>Internal Server Error</h1>
               <p>Failed to initialize the .NET application.</p>
               </body>
            </html>
            """u8.ToArray();

    private nint _hostContextHandle;
    private CallbackState _callbackState = new();
    private readonly TaskCompletionSource _initializationTcs = new();
    private readonly SemaphoreSlim initLock = new(1);

    public unsafe REQUEST_NOTIFICATION_STATUS OnExecuteRequestHandler(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return (REQUEST_NOTIFICATION_STATUS)_callbackState.RequestCallback(_callbackState.Context, (IntPtr)pHttpContext, (IntPtr)pProvider);
    }

    public unsafe REQUEST_NOTIFICATION_STATUS OnAsyncCompletion(IHttpContext* pHttpContext, uint dwNotification, BOOL fPostNotification, IHttpEventProvider* pProvider, IHttpCompletionInfo* pCompletionInfo)
    {
        return (REQUEST_NOTIFICATION_STATUS)_callbackState.AsyncCallback(_callbackState.Context, (IntPtr)pHttpContext, dwNotification, (int)fPostNotification, (IntPtr)pProvider, (IntPtr)pCompletionInfo);
    }

    internal unsafe static void RegisterCallbacks(
        delegate* unmanaged<IntPtr, IntPtr, IntPtr, int> requestCallback,
        delegate* unmanaged<IntPtr, IntPtr, uint, int, IntPtr, IntPtr, int> asyncCallback,
        IntPtr pContext)
    {
        if (!s_instance._initialized)
        {
            throw new InvalidOperationException("CLRHost not initialized");
        }

        s_instance._callbackState = new CallbackState
        {
            RequestCallback = requestCallback,
            AsyncCallback = asyncCallback,
            Context = pContext,
        };

        s_instance._initializationTcs.TrySetResult();
    }

    [UnmanagedCallersOnly]
    public unsafe static int ErrorPage(IntPtr _, IntPtr pHttpContext, IntPtr pModuleInfo)
    {
        var httpContext = (IHttpContext*)pHttpContext;
        var pResponse = httpContext->GetResponse();
        pResponse->Clear();

        // These strings must be null-terminated
        fixed (byte* statusDescription = "Internal Server Error\u0000"u8)
        {
            pResponse->SetStatus(500, (sbyte*)statusDescription);
        }

        fixed (byte* headerName = "Content-Type\u0000"u8)
        fixed (byte* headerValue = "text/html\u0000"u8)
        {
            pResponse->SetHeader((sbyte*)headerName, (sbyte*)headerValue, 9, fReplace: true);
        }

        HTTP_DATA_CHUNK chunk = default;
        chunk.DataChunkType = HTTP_DATA_CHUNK_TYPE.HttpDataChunkFromMemory;

        fixed (byte* pBody = s_instance._error)
        {
            chunk.FromMemory.pBuffer = pBody;
            chunk.FromMemory.BufferLength = (uint)s_instance._error.Length;

            uint bytesSent;
            pResponse->WriteEntityChunks(&chunk, 1, fAsync: false, fMoreData: false, &bytesSent);
        }

        return (int)REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_FINISH_REQUEST;
    }

    public static ValueTask<CLRHost> GetOrCreateAsync()
    {
        if (s_instance._initialized)
        {
            return ValueTask.FromResult(s_instance);
        }

        return Core();

        static async ValueTask<CLRHost> Core()
        {
            // This lock stops multiple threads from initializing the CLRHost at the same time
            await s_instance.initLock.WaitAsync();

            try
            {
                if (s_instance._initialized)
                {
                    return s_instance;
                }

                // See https://github.com/dotnet/runtime/blob/main/docs/design/features/native-hosting.md for more information on native hosting in .NET.

                if (!OperatingSystem.IsWindows())
                {
                    s_instance._error = Encoding.UTF8.GetBytes($"{RuntimeInformation.RuntimeIdentifier} is unsupported.");
                    s_instance._initialized = true;
                    return s_instance;
                }

                // Uncomment to debug
                // Environment.SetEnvironmentVariable("COREHOST_TRACE", "1");
                // Environment.SetEnvironmentVariable("COREHOST_TRACE_VERBOSITY", "4");

                // TODO: Look at the path
                var dotnetRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet");

                var allHostFxrDirs = new DirectoryInfo(Path.Combine(dotnetRoot, "host\\fxr"));
                var hostFxrDirectory = allHostFxrDirs.EnumerateDirectories().FirstOrDefault(d => d.Name.StartsWith("9.0"));

                if (hostFxrDirectory is null)
                {
                    s_instance._error = Encoding.UTF8.GetBytes($"Unable to find 9.0.x hostfxr: {string.Join(Environment.NewLine, allHostFxrDirs)}");
                    s_instance._initialized = true;
                    return s_instance;
                }

                // Console.WriteLine($"Using hostfxr: {hostFxrDirectory}");

                NativeLibrary.Load(Path.Combine(hostFxrDirectory.FullName, "hostfxr.dll"));

                var dll = Environment.GetEnvironmentVariable("DOTNET_DLL");

                if (string.IsNullOrEmpty(dll))
                {
                    s_instance._error = "DOTNET_DLL environment variable is not set."u8.ToArray();
                    s_instance._initialized = true;
                    return s_instance;
                }

                string[] args = [dll!];

                unsafe
                {
                    fixed (char* hostPathPointer = Environment.CurrentDirectory)
                    fixed (char* dotnetRootPointer = dotnetRoot)
                    {
                        var parameters = new HostFxrImports.hostfxr_initialize_parameters
                        {
                            size = sizeof(HostFxrImports.hostfxr_initialize_parameters),
                            host_path = hostPathPointer,
                            dotnet_root = dotnetRootPointer
                        };

                        var err = HostFxrImports.Initialize(args.Length, args, ref parameters, out var host_context_handle);

                        if (err < 0)
                        {
                            s_instance._returnCode = err;
                            s_instance._error = Encoding.UTF8.GetBytes($"Error initializing hostfxr {err}");
                            s_instance._initialized = true;
                            return s_instance;
                        }

                        s_instance._hostContextHandle = host_context_handle;
                        // Once we get the handle, we're initialized
                        s_instance._initialized = true;

                        var thread = new Thread(static state =>
                        {
                            var host = (CLRHost)state!;
                            int val = HostFxrImports.Run(host._hostContextHandle);
                            host._returnCode = val;
                        })
                        {
                            IsBackground = true
                        };

                        thread.Start(s_instance);
                    }
                }

                try
                {
                    await s_instance._initializationTcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
                }
                catch (TimeoutException)
                {
                    // Timed out
                }

                return s_instance;
            }
            finally
            {
                s_instance.initLock.Release();
            }
        }
    }
}
