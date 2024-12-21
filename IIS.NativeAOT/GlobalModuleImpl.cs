using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace IIS.NativeAOT;

internal unsafe struct GlobalModuleImpl : CGlobalModule.Interface
{
    private CGlobalModule.Vtbl<GlobalModuleImpl>* lpVtbl;

    private static readonly CGlobalModule.Vtbl<GlobalModuleImpl>* VtblInstance = InitVtblInstance();

    private static unsafe CGlobalModule.Vtbl<GlobalModuleImpl>* InitVtblInstance()
    {
        var lpVtbl = (CGlobalModule.Vtbl<GlobalModuleImpl>*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(GlobalModuleImpl), sizeof(CGlobalModule.Vtbl<GlobalModuleImpl>));

        // Implement vtable here
        lpVtbl->OnGlobalApplicationPreload = &OnGlobalApplicationPreload;
        lpVtbl->OnGlobalApplicationResolveModules = &OnGlobalApplicationResolveModules;
        lpVtbl->OnGlobalApplicationStart = &OnGlobalApplicationStart;
        lpVtbl->OnGlobalApplicationStop = &OnGlobalApplicationStop;
        lpVtbl->OnGlobalCacheCleanup = &OnGlobalCacheCleanup;
        lpVtbl->OnGlobalCacheOperation = &OnGlobalCacheOperation;
        lpVtbl->OnGlobalConfigurationChange = &OnGlobalConfigurationChange;
        lpVtbl->OnGlobalCustomNotification = &OnGlobalCustomNotification;
        lpVtbl->OnGlobalFileChange = &OnGlobalFileChange;
        lpVtbl->OnGlobalHealthCheck = &OnGlobalHealthCheck;
        lpVtbl->OnGlobalPreBeginRequest = &OnGlobalPreBeginRequest;
        lpVtbl->OnGlobalRSCAQuery = &OnGlobalRSCAQuery;
        lpVtbl->OnGlobalStopListening = &OnGlobalStopListening;
        lpVtbl->OnGlobalThreadCleanup = &OnGlobalThreadCleanup;
        lpVtbl->OnGlobalTraceEvent = &OnGlobalTraceEvent;
        lpVtbl->OnSuspendProcess = &OnSuspendProcess;
        lpVtbl->Terminate = &Terminate;

        return lpVtbl;
    }

    public static CGlobalModule* Create()
    {
        var pHttpModuleFactory = (GlobalModuleImpl*)NativeMemory.Alloc((uint)sizeof(GlobalModuleImpl));
        pHttpModuleFactory->lpVtbl = VtblInstance;
        return (CGlobalModule*)pHttpModuleFactory;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static GLOBAL_NOTIFICATION_STATUS OnGlobalApplicationPreload(GlobalModuleImpl* self, IGlobalApplicationPreloadProvider* pProvider)
    {
        return self->OnGlobalApplicationPreload(pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static GLOBAL_NOTIFICATION_STATUS OnGlobalApplicationResolveModules(GlobalModuleImpl* self, IHttpApplicationResolveModulesProvider* pProvider)
    {
        return self->OnGlobalApplicationResolveModules(pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static GLOBAL_NOTIFICATION_STATUS OnGlobalApplicationStart(GlobalModuleImpl* self, IHttpApplicationProvider* pProvider)
    {
        return self->OnGlobalApplicationStart(pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static GLOBAL_NOTIFICATION_STATUS OnGlobalApplicationStop(GlobalModuleImpl* self, IHttpApplicationProvider* pProvider)
    {
        return self->OnGlobalApplicationStop(pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static GLOBAL_NOTIFICATION_STATUS OnGlobalCacheCleanup(GlobalModuleImpl* self)
    {
        return self->OnGlobalCacheCleanup();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static GLOBAL_NOTIFICATION_STATUS OnGlobalCacheOperation(GlobalModuleImpl* self, ICacheProvider* pProvider)
    {
        return self->OnGlobalCacheOperation(pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static GLOBAL_NOTIFICATION_STATUS OnGlobalConfigurationChange(GlobalModuleImpl* self, IGlobalConfigurationChangeProvider* pProvider)
    {
        return self->OnGlobalConfigurationChange(pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static GLOBAL_NOTIFICATION_STATUS OnGlobalCustomNotification(GlobalModuleImpl* self, ICustomNotificationProvider* pProvider)
    {
        return self->OnGlobalCustomNotification(pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static GLOBAL_NOTIFICATION_STATUS OnGlobalFileChange(GlobalModuleImpl* self, IGlobalFileChangeProvider* pProvider)
    {
        return self->OnGlobalFileChange(pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static GLOBAL_NOTIFICATION_STATUS OnGlobalHealthCheck(GlobalModuleImpl* self)
    {
        return self->OnGlobalHealthCheck();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static GLOBAL_NOTIFICATION_STATUS OnGlobalPreBeginRequest(GlobalModuleImpl* self, IPreBeginRequestProvider* pProvider)
    {
        return self->OnGlobalPreBeginRequest(pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static GLOBAL_NOTIFICATION_STATUS OnGlobalRSCAQuery(GlobalModuleImpl* self, IGlobalRSCAQueryProvider* pProvider)
    {
        return self->OnGlobalRSCAQuery(pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static GLOBAL_NOTIFICATION_STATUS OnGlobalStopListening(GlobalModuleImpl* self, IGlobalStopListeningProvider* pProvider)
    {
        return self->OnGlobalStopListening(pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static GLOBAL_NOTIFICATION_STATUS OnGlobalThreadCleanup(GlobalModuleImpl* self, IGlobalThreadCleanupProvider* pProvider)
    {
        return self->OnGlobalThreadCleanup(pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static GLOBAL_NOTIFICATION_STATUS OnGlobalTraceEvent(GlobalModuleImpl* self, IGlobalTraceEventProvider* pProvider)
    {
        return self->OnGlobalTraceEvent(pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static GLOBAL_NOTIFICATION_STATUS OnSuspendProcess(GlobalModuleImpl* self, IGlobalSuspendProcessCallback** pCallback)
    {
        return self->OnSuspendProcess(pCallback);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static void Terminate(GlobalModuleImpl* self)
    {
        self->Terminate();

        NativeMemory.Free(self);
    }

    public unsafe GLOBAL_NOTIFICATION_STATUS OnGlobalApplicationPreload(IGlobalApplicationPreloadProvider* pProvider)
    {
        return GLOBAL_NOTIFICATION_STATUS.GL_NOTIFICATION_CONTINUE;
    }

    public unsafe GLOBAL_NOTIFICATION_STATUS OnGlobalApplicationResolveModules(IHttpApplicationResolveModulesProvider* pProvider)
    {
        return GLOBAL_NOTIFICATION_STATUS.GL_NOTIFICATION_CONTINUE;
    }

    public unsafe GLOBAL_NOTIFICATION_STATUS OnGlobalApplicationStart(IHttpApplicationProvider* pProvider)
    {
        return GLOBAL_NOTIFICATION_STATUS.GL_NOTIFICATION_CONTINUE;
    }

    public unsafe GLOBAL_NOTIFICATION_STATUS OnGlobalApplicationStop(IHttpApplicationProvider* pProvider)
    {
        return GLOBAL_NOTIFICATION_STATUS.GL_NOTIFICATION_CONTINUE;
    }

    public GLOBAL_NOTIFICATION_STATUS OnGlobalCacheCleanup()
    {
        return GLOBAL_NOTIFICATION_STATUS.GL_NOTIFICATION_CONTINUE;
    }

    public unsafe GLOBAL_NOTIFICATION_STATUS OnGlobalCacheOperation(ICacheProvider* pProvider)
    {
        return GLOBAL_NOTIFICATION_STATUS.GL_NOTIFICATION_CONTINUE;
    }

    public unsafe GLOBAL_NOTIFICATION_STATUS OnGlobalConfigurationChange(IGlobalConfigurationChangeProvider* pProvider)
    {
        return GLOBAL_NOTIFICATION_STATUS.GL_NOTIFICATION_CONTINUE;
    }

    public unsafe GLOBAL_NOTIFICATION_STATUS OnGlobalCustomNotification(ICustomNotificationProvider* pProvider)
    {
        return GLOBAL_NOTIFICATION_STATUS.GL_NOTIFICATION_CONTINUE;
    }

    public unsafe GLOBAL_NOTIFICATION_STATUS OnGlobalFileChange(IGlobalFileChangeProvider* pProvider)
    {
        return GLOBAL_NOTIFICATION_STATUS.GL_NOTIFICATION_CONTINUE;
    }

    public GLOBAL_NOTIFICATION_STATUS OnGlobalHealthCheck()
    {
        return GLOBAL_NOTIFICATION_STATUS.GL_NOTIFICATION_CONTINUE;
    }

    public unsafe GLOBAL_NOTIFICATION_STATUS OnGlobalPreBeginRequest(IPreBeginRequestProvider* pProvider)
    {
        return GLOBAL_NOTIFICATION_STATUS.GL_NOTIFICATION_CONTINUE;
    }

    public unsafe GLOBAL_NOTIFICATION_STATUS OnGlobalRSCAQuery(IGlobalRSCAQueryProvider* pProvider)
    {
        return GLOBAL_NOTIFICATION_STATUS.GL_NOTIFICATION_CONTINUE;
    }

    public unsafe GLOBAL_NOTIFICATION_STATUS OnGlobalStopListening(IGlobalStopListeningProvider* pProvider)
    {
        return GLOBAL_NOTIFICATION_STATUS.GL_NOTIFICATION_CONTINUE;
    }

    public unsafe GLOBAL_NOTIFICATION_STATUS OnGlobalThreadCleanup(IGlobalThreadCleanupProvider* pProvider)
    {
        return GLOBAL_NOTIFICATION_STATUS.GL_NOTIFICATION_CONTINUE;
    }

    public unsafe GLOBAL_NOTIFICATION_STATUS OnGlobalTraceEvent(IGlobalTraceEventProvider* pProvider)
    {
        return GLOBAL_NOTIFICATION_STATUS.GL_NOTIFICATION_CONTINUE;
    }

    public unsafe GLOBAL_NOTIFICATION_STATUS OnSuspendProcess(IGlobalSuspendProcessCallback** pCallback)
    {
        return GLOBAL_NOTIFICATION_STATUS.GL_NOTIFICATION_CONTINUE;
    }

    public void Terminate()
    {
        
    }
}
