using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace IIS.NativeAOT;

internal class CLRHost
{
    private static readonly CLRHost s_instance = new();
    private static readonly byte[] s_defaultErrorPage =
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

    private static readonly TimeSpan s_managedApplicationTimeout = TimeSpan.FromSeconds(5);

    // The return code from the hostfxr_run call or the error code from initialization
    private int _returnCode;

    // The handle to the host context, used to run the application
    private nint _hostContextHandle;

    // The managed application instance, this is guaranteed to be set after initialization either by the managed code calling
    // RegisterCallbacks or by the initialization code because of an error;
    private readonly TaskCompletionSource<ManagedApplication> _managedApplicationTcs = new();
    private readonly SemaphoreSlim _initLock = new(1);

    private bool IsInitialized => _managedApplicationTcs.Task.IsCompleted;

    private ManagedApplication ManagedApplication
    {
        get
        {
            Debug.Assert(IsInitialized);
            return _managedApplicationTcs.Task.Result;
        }
    }

    internal unsafe static void RegisterCallbacks(
        delegate* unmanaged<IntPtr, IntPtr, IntPtr, int> requestCallback,
        delegate* unmanaged<IntPtr, IntPtr, uint, int, IntPtr, IntPtr, int> asyncCallback,
        IntPtr pContext)
    {
        if (!s_instance._managedApplicationTcs.TrySetResult(new ManagedApplication(requestCallback, asyncCallback, pContext)))
        {
            throw new InvalidOperationException("Managed application already initialized.");
        }
    }

    private ManagedApplication SetApplicationFailed(string? error = null)
    {
        var errorBytes = error is null ? s_defaultErrorPage : Encoding.UTF8.GetBytes(error);
        _managedApplicationTcs.TrySetResult(new ManagedApplication(errorBytes));
        return ManagedApplication;
    }

    public static ValueTask<ManagedApplication> GetOrCreateAsync()
    {
        if (s_instance.IsInitialized)
        {
            return ValueTask.FromResult(s_instance.ManagedApplication);
        }

        return Core();

        static async ValueTask<ManagedApplication> Core()
        {
            // This lock stops multiple threads from initializing the CLRHost at the same time
            await s_instance._initLock.WaitAsync();

            try
            {
                if (s_instance.IsInitialized)
                {
                    return s_instance.ManagedApplication;
                }

                // See https://github.com/dotnet/runtime/blob/main/docs/design/features/native-hosting.md for more information on native hosting in .NET.

                if (!OperatingSystem.IsWindows())
                {
                    return s_instance.SetApplicationFailed($"{RuntimeInformation.RuntimeIdentifier} is unsupported.");
                }

                // Uncomment to debug
                // Environment.SetEnvironmentVariable("COREHOST_TRACE", "1");
                // Environment.SetEnvironmentVariable("COREHOST_TRACE_VERBOSITY", "4");

                static string? GetDotnetRootPath()
                {
                    // Check the DOTNET_ROOT environment variable
                    var dotnetRootEnv = Environment.GetEnvironmentVariable("DOTNET_ROOT");
                    if (!string.IsNullOrEmpty(dotnetRootEnv) && Directory.Exists(dotnetRootEnv))
                    {
                        return dotnetRootEnv;
                    }

                    // Check the default installation path for .NET
                    var programFilesDotnet = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet");
                    if (Directory.Exists(programFilesDotnet))
                    {
                        return programFilesDotnet;
                    }

                    // Search for dotnet.exe in the PATH environment variable
                    var pathEnvironmentVariable = Environment.GetEnvironmentVariable("PATH");
                    if (pathEnvironmentVariable is null)
                    {
                        return null;
                    }

                    foreach (string path in pathEnvironmentVariable.Split(Path.PathSeparator))
                    {
                        string potentialDotnetPath = Path.Combine(path, "dotnet");
                        if (File.Exists(Path.Combine(potentialDotnetPath, "dotnet.exe")))
                        {
                            return potentialDotnetPath;
                        }
                    }

                    return null;
                }

                var dotnetRoot = GetDotnetRootPath();

                if (dotnetRoot is null)
                {
                    return s_instance.SetApplicationFailed("Unable to find .NET installation.");
                }

                var allHostFxrDirs = new DirectoryInfo(Path.Combine(dotnetRoot, "host", "fxr"));

                // REVIEW: Should we parse the versions and pick the latest properly?
                var hostFxrDirectory = (from d in allHostFxrDirs.EnumerateDirectories()
                                        let version = FxVer.Parse(d.Name)
                                        orderby version descending
                                        select d)
                                        .FirstOrDefault();

                if (hostFxrDirectory is null)
                {
                    return s_instance.SetApplicationFailed($"Unable to find hostfxr in {dotnetRoot}");
                }

                // Console.WriteLine($"Using hostfxr: {hostFxrDirectory}");

                NativeLibrary.Load(Path.Combine(hostFxrDirectory.FullName, "hostfxr.dll"));

                var dll = Environment.GetEnvironmentVariable("DOTNET_DLL");

                if (string.IsNullOrEmpty(dll))
                {
                    return s_instance.SetApplicationFailed("DOTNET_DLL environment variable is not set.");
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
                            return s_instance.SetApplicationFailed($"Error initializing hostfxr {err}");
                        }

                        s_instance._hostContextHandle = host_context_handle;

                        // We spin up a new thread to run the hostfxr_run loop
                        // this is because we're calling into main which is blocking
                        // We don't want to block the IIS thread so we run the application outside of this thread
                        // and watch for completion.
                        var thread = new Thread(static state =>
                        {
                            var host = (CLRHost)state!;
                            host._returnCode = HostFxrImports.Run(host._hostContextHandle);

                            // TODO: When the application shuts down, this app pool should shut down as well
                        })
                        {
                            IsBackground = true
                        };

                        thread.Start(s_instance);
                    }
                }

                try
                {
                    return await s_instance._managedApplicationTcs.Task.WaitAsync(s_managedApplicationTimeout);
                }
                catch (TimeoutException)
                {
                    // Timed out
                    return s_instance.SetApplicationFailed("Managed application initialization timed out.");
                }
            }
            finally
            {
                s_instance._initLock.Release();
            }
        }
    }
}
