using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace IIS.NativeAOT;

internal unsafe class CLRHost
{
    private static readonly Lock s_lock = new();
    private static bool s_initialized = false;
    private static CLRHost s_instance;

    // Instance state
    private delegate* unmanaged<IntPtr, IntPtr, IntPtr, int> RequestCallback = &ErrorPage;
    private delegate* unmanaged<IntPtr, IntPtr, uint, int, IntPtr, IntPtr, int> AsyncCallback;
    private IntPtr Context;

    private int _returnCode;
    private nint _hostContextHandle;
    private readonly ManualResetEventSlim _wh = new();

    public REQUEST_NOTIFICATION_STATUS OnExecuteRequestHandler(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        if (RequestCallback is null)
        {
            throw new InvalidOperationException("RequestCallback not set");
        }

        return (REQUEST_NOTIFICATION_STATUS)RequestCallback(Context, (IntPtr)pHttpContext, (IntPtr)pProvider);
    }

    public REQUEST_NOTIFICATION_STATUS OnAsyncCompletion(IHttpContext* pHttpContext, uint dwNotification, BOOL fPostNotification, IHttpEventProvider* pProvider, IHttpCompletionInfo* pCompletionInfo)
    {
        if (AsyncCallback is null)
        {
            throw new InvalidOperationException("AsyncCallback not set");
        }

        return (REQUEST_NOTIFICATION_STATUS)AsyncCallback(Context, (IntPtr)pHttpContext, dwNotification, (int)fPostNotification, (IntPtr)pProvider, (IntPtr)pCompletionInfo);
    }

    internal static void RegisterCallbacks(
        delegate* unmanaged<IntPtr, IntPtr, IntPtr, int> requestCallback, 
        delegate* unmanaged<IntPtr, IntPtr, uint, int, IntPtr, IntPtr, int> asyncCallback,
        IntPtr pContext)
    {
        if (s_instance is null)
        {
            throw new InvalidOperationException("CLRHost not initialized");
        }

        s_instance.RequestCallback = requestCallback;
        s_instance.AsyncCallback = asyncCallback;
        s_instance.Context = pContext;
        s_instance._wh.Set();
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

        var body =
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
            """u8;

        HTTP_DATA_CHUNK chunk = default;
        chunk.DataChunkType = HTTP_DATA_CHUNK_TYPE.HttpDataChunkFromMemory;

        fixed (byte* pBody = body)
        {
            chunk.FromMemory.pBuffer = pBody;
            chunk.FromMemory.BufferLength = (uint)body.Length;

            uint bytesSent;
            pResponse->WriteEntityChunks(&chunk, 1, fAsync: false, fMoreData: false, &bytesSent);
        }

        return (int)REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_FINISH_REQUEST;
    }

    public static CLRHost GetOrCreate()
    {
        lock (s_lock)
        {
            if (s_initialized)
            {
                return s_instance;
            }

            // See https://github.com/dotnet/runtime/blob/main/docs/design/features/native-hosting.md for more information on native hosting in .NET.

            // TODO: Support other platforms

            if (!OperatingSystem.IsWindows())
            {
                Console.WriteLine($"{RuntimeInformation.RuntimeIdentifier} is unsupported.");
                s_instance = new CLRHost { _returnCode = 1 };
                return s_instance;
            }

            // Uncomment to debug
            // Environment.SetEnvironmentVariable("COREHOST_TRACE", "1");
            // Environment.SetEnvironmentVariable("COREHOST_TRACE_VERBOSITY", "4");

            var dotnetRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet");

            var allHostFxrDirs = new DirectoryInfo(Path.Combine(dotnetRoot, "host\\fxr"));
            var hostFxrDirectory = allHostFxrDirs.EnumerateDirectories().FirstOrDefault(d => d.Name.StartsWith("9.0"));

            if (hostFxrDirectory is null)
            {
                Console.WriteLine($"Unable to find 9.0.x hostfxr: {string.Join(Environment.NewLine, allHostFxrDirs)}");
                s_instance = new CLRHost { _returnCode = -1 };
                return s_instance;
            }

            // Console.WriteLine($"Using hostfxr: {hostFxrDirectory}");

            NativeLibrary.Load(Path.Combine(hostFxrDirectory.FullName, "hostfxr.dll"));

            var dll = Environment.GetEnvironmentVariable("DOTNET_DLL");

            if (string.IsNullOrEmpty(dll))
            {
                Console.WriteLine("DOTNET_DLL environment variable is not set.");
                s_instance = new CLRHost { _returnCode = -1 };
                return s_instance;
            }

            string[] args = [dll!];

            unsafe
            {
                //var d = Path.GetDirectoryName(targetPath)!;

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
                        Console.WriteLine($"Error invoking initialize {err}");
                        s_instance = new CLRHost { _returnCode = err };
                        return s_instance;
                    }

                    s_instance = new CLRHost
                    {
                        _hostContextHandle = host_context_handle
                    };

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

                    if (!s_instance._wh.Wait(TimeSpan.FromSeconds(5)))
                    {
                        s_instance = new CLRHost { _returnCode = -1 };
                        return s_instance;
                    }

                    s_initialized = true;

                    return s_instance;
                }
            }
        }
    }
}
