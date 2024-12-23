using System.Diagnostics;
using System.Text;

namespace IIS.NativeAOT;

internal sealed class CLRHost
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
    private int? _returnCode;

    // The handle to the host context, used to run the application
    private nint _hostContextHandle;

    // The managed application instance, this is guaranteed to be set after initialization either by the managed code calling
    // RegisterCallbacks or by the initialization code because of an error;
    private readonly TaskCompletionSource<ManagedApplication> _managedApplicationTcs = new();
    private readonly SemaphoreSlim _initLock = new(1);

    private bool IsInitialized => _managedApplicationTcs.Task.IsCompleted;

    internal unsafe static void RegisterCallbacks(
        delegate* unmanaged<IntPtr, IntPtr, IntPtr, int> requestCallback,
        delegate* unmanaged<IntPtr, IntPtr, uint, int, IntPtr, IntPtr, int> asyncCallback,
        IntPtr pContext)
    {
        if (!s_instance._managedApplicationTcs.TrySetResult(new ManagedApplication(requestCallback, asyncCallback, pContext)))
        {
            // TODO: Return an error code
            throw new InvalidOperationException("Managed application already initialized.");
        }
    }

    private Task<ManagedApplication> SetApplicationFailed(string? error = null)
    {
        var errorBytes = error is null ? s_defaultErrorPage : Encoding.UTF8.GetBytes(error);
        _managedApplicationTcs.TrySetResult(new ManagedApplication(errorBytes));
        return _managedApplicationTcs.Task;
    }

    public static Task<ManagedApplication> GetOrCreateAsync()
    {
        // If the instance is already initialized we can just return the task
        if (s_instance.IsInitialized || !s_instance._initLock.Wait(0))
        {
            return s_instance._managedApplicationTcs.Task;
        }

        // _initLock stops multiple threads from initializing the CLRHost at the same time
        // This is split into a local method to avoid the state machine in the fast path
        return CreateApplicationAsync();

        static async Task<ManagedApplication> CreateApplicationAsync()
        {
            // This method assumes we have the semaphore
            try
            {
                if (s_instance.IsInitialized)
                {
                    return await s_instance._managedApplicationTcs.Task;
                }

                var dll = Environment.GetEnvironmentVariable("DOTNET_DLL");

                var (error, errorCode, handle) = HostFxr.Initialize(dll);

                if (error is not null)
                {
                    s_instance._returnCode = errorCode;

                    return await s_instance.SetApplicationFailed(error);
                }
                else
                {
                    s_instance._hostContextHandle = handle;

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

                    // We boot the managed application and wait for it to call back into RegisterCallbacks
                    // if it doesn't within the timeout we fail the application initialization and set the error page
                    try
                    {
                        return await s_instance._managedApplicationTcs.Task.WaitAsync(s_managedApplicationTimeout);
                    }
                    catch (TimeoutException)
                    {
                        // Timed out
                        return await s_instance.SetApplicationFailed("Managed application initialization timed out.");
                    }
                }
            }
            finally
            {
                Debug.Assert(s_instance.IsInitialized, "The CLR host isn't fully initialized");

                s_instance._initLock.Release();
            }
        }
    }
}
