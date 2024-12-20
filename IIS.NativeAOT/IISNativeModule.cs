using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TerraFX.Interop.Windows;

namespace IIS.NativeAOT;

[SupportedOSPlatform("windows")]
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

        return 0; // Success
    }
}
