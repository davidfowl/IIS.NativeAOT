using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace IIS.NativeAOT;

public static class IISNativeModule
{
    [UnmanagedCallersOnly(EntryPoint = "RegisterModule")]
    public unsafe static int RegisterModule(uint serverVersion, IntPtr moduleInfo, IntPtr globalInfo)
    {
        var registrationInfo = (IHttpModuleRegistrationInfo*)moduleInfo;

        var factory = HttpModuleFactoryImpl.Create();

        registrationInfo->SetRequestNotifications(factory,
            (uint)RequestNotifications.RQ_EXECUTE_REQUEST_HANDLER,
            0);

        var globalModule = GlobalModuleImpl.Create();

        registrationInfo->SetGlobalNotifications(globalModule,
            (uint)(GlobalNotifications.GL_CONFIGURATION_CHANGE | // Configuration change triggers IIS application stop
                   GlobalNotifications.GL_STOP_LISTENING |       // worker process will stop listening for http requests
                   GlobalNotifications.GL_APPLICATION_STOP));    // app pool recycle or stop)

        return 0; // Success
    }

    [UnmanagedCallersOnly(EntryPoint = "RegisterCallbacks")]
    public unsafe static int RegisterCallbacks(
        delegate* unmanaged<IntPtr, IntPtr, IntPtr, int> requestCallback,
        delegate* unmanaged<IntPtr, IntPtr, uint, int, IntPtr, IntPtr, int> asyncCallback,
        IntPtr context)
    {
        return CLRHost.RegisterCallbacks(requestCallback, asyncCallback, context);
    }
}
