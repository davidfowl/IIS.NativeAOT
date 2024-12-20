using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using TerraFX.Interop.Windows;

namespace IIS.NativeAOT;

[SupportedOSPlatform("windows")]
internal unsafe struct HttpModuleImpl : CHttpModule.Interface
{
    private CHttpModule.Vtbl<HttpModuleImpl>* lpVtbl;

    private static readonly CHttpModule.Vtbl<HttpModuleImpl>* VtblInstance = InitVtblInstance();

    private static CHttpModule.Vtbl<HttpModuleImpl>* InitVtblInstance()
    {
        CHttpModule.Vtbl<HttpModuleImpl>* lpVtbl = (CHttpModule.Vtbl<HttpModuleImpl>*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(HttpModuleImpl), sizeof(CHttpModule.Vtbl<HttpModuleImpl>));

        // Implement the vtable here

        lpVtbl->Destructor = &Destructor;
        lpVtbl->Dispose = &Dispose;
        lpVtbl->OnAcquireRequestState = &OnAcquireRequestState;
        lpVtbl->OnAsyncCompletion = &OnAsyncCompletion;
        lpVtbl->OnAuthenticateRequest = &OnAuthenticateRequest;
        lpVtbl->OnAuthorizeRequest = &OnAuthorizeRequest;
        lpVtbl->OnBeginRequest = &OnBeginRequest;
        lpVtbl->OnCustomRequestNotification = &OnCustomRequestNotification;
        lpVtbl->OnEndRequest = &OnEndRequest;
        lpVtbl->OnExecuteRequestHandler = &OnExecuteRequestHandler;
        lpVtbl->OnLogRequest = &OnLogRequest;
        lpVtbl->OnMapPath = &OnMapPath;
        lpVtbl->OnMapRequestHandler = &OnMapRequestHandler;
        lpVtbl->OnPostAcquireRequestState = &OnPostAcquireRequestState;
        lpVtbl->OnPostAuthenticateRequest = &OnPostAuthenticateRequest;
        lpVtbl->OnPostAuthorizeRequest = &OnPostAuthorizeRequest;
        lpVtbl->OnPostBeginRequest = &OnPostBeginRequest;
        lpVtbl->OnPostEndRequest = &OnPostEndRequest;
        lpVtbl->OnPostExecuteRequestHandler = &OnPostExecuteRequestHandler;
        lpVtbl->OnPostLogRequest = &OnPostLogRequest;
        lpVtbl->OnPostMapRequestHandler = &OnPostMapRequestHandler;
        lpVtbl->OnPostPreExecuteRequestHandler = &OnPostPreExecuteRequestHandler;
        lpVtbl->OnPostReleaseRequestState = &OnPostReleaseRequestState;
        lpVtbl->OnPostResolveRequestCache = &OnPostResolveRequestCache;
        lpVtbl->OnPostUpdateRequestCache = &OnPostUpdateRequestCache;
        lpVtbl->OnPreExecuteRequestHandler = &OnPreExecuteRequestHandler;
        lpVtbl->OnReadEntity = &OnReadEntity;
        lpVtbl->OnReleaseRequestState = &OnReleaseRequestState;
        lpVtbl->OnResolveRequestCache = &OnResolveRequestCache;
        lpVtbl->OnSendResponse = &OnSendResponse;
        lpVtbl->OnUpdateRequestCache = &OnUpdateRequestCache;

        return lpVtbl;
    }
    public static CHttpModule* Create(IModuleAllocator* pAllocator)
    {
        HttpModuleImpl* pHttpModuleFactory = (HttpModuleImpl*)pAllocator->AllocateMemory((uint)sizeof(HttpModuleImpl));
        pHttpModuleFactory->lpVtbl = VtblInstance;
        return (CHttpModule*)pHttpModuleFactory;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static void Destructor(HttpModuleImpl* self)
    {
        // Implement the destructor here
        self->Destructor();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static void Dispose(HttpModuleImpl* self)
    {
        self->Dispose();
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnAcquireRequestState(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnAcquireRequestState(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnAsyncCompletion(HttpModuleImpl* self, IHttpContext* pHttpContext, uint dwNotification, BOOL fPostNotification, IHttpEventProvider* pProvider, IHttpCompletionInfo* pCompletionInfo)
    {
        return self->OnAsyncCompletion(pHttpContext, dwNotification, fPostNotification, pProvider, pCompletionInfo);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnAuthenticateRequest(HttpModuleImpl* self, IHttpContext* pHttpContext, IAuthenticationProvider* pProvider)
    {
        return self->OnAuthenticateRequest(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnAuthorizeRequest(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnAuthorizeRequest(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnBeginRequest(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnBeginRequest(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnCustomRequestNotification(HttpModuleImpl* self, IHttpContext* pHttpContext, ICustomNotificationProvider* pProvider)
    {
        return self->OnCustomRequestNotification(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnEndRequest(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnEndRequest(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnExecuteRequestHandler(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnExecuteRequestHandler(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnLogRequest(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnLogRequest(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnMapPath(HttpModuleImpl* self, IHttpContext* pHttpContext, IMapPathProvider* pProvider)
    {
        return self->OnMapPath(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnMapRequestHandler(HttpModuleImpl* self, IHttpContext* pHttpContext, IMapHandlerProvider* pProvider)
    {
        return self->OnMapRequestHandler(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnPostAcquireRequestState(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnPostAcquireRequestState(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnPostAuthenticateRequest(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnPostAuthenticateRequest(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnPostAuthorizeRequest(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnPostAuthorizeRequest(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnPostBeginRequest(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnPostBeginRequest(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnPostEndRequest(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnPostEndRequest(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnPostExecuteRequestHandler(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnPostExecuteRequestHandler(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnPostLogRequest(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnPostLogRequest(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnPostMapRequestHandler(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnPostMapRequestHandler(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnPostPreExecuteRequestHandler(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnPostPreExecuteRequestHandler(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnPostReleaseRequestState(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnPostReleaseRequestState(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnPostResolveRequestCache(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnPostResolveRequestCache(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnPostUpdateRequestCache(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnPostUpdateRequestCache(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnPreExecuteRequestHandler(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnPreExecuteRequestHandler(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnReadEntity(HttpModuleImpl* self, IHttpContext* pHttpContext, IReadEntityProvider* pProvider)
    {
        return self->OnReadEntity(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnReleaseRequestState(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnReleaseRequestState(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnResolveRequestCache(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnResolveRequestCache(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnSendResponse(HttpModuleImpl* self, IHttpContext* pHttpContext, ISendResponseProvider* pProvider)
    {
        return self->OnSendResponse(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static REQUEST_NOTIFICATION_STATUS OnUpdateRequestCache(HttpModuleImpl* self, IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return self->OnUpdateRequestCache(pHttpContext, pProvider);
    }

    public REQUEST_NOTIFICATION_STATUS OnBeginRequest(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnPostBeginRequest(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnAuthenticateRequest(IHttpContext* pHttpContext, IAuthenticationProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnPostAuthenticateRequest(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnAuthorizeRequest(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnPostAuthorizeRequest(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnResolveRequestCache(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnPostResolveRequestCache(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnMapRequestHandler(IHttpContext* pHttpContext, IMapHandlerProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnPostMapRequestHandler(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnAcquireRequestState(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnPostAcquireRequestState(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnPreExecuteRequestHandler(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnPostPreExecuteRequestHandler(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnExecuteRequestHandler(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnPostExecuteRequestHandler(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnReleaseRequestState(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnPostReleaseRequestState(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnUpdateRequestCache(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnPostUpdateRequestCache(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnLogRequest(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnPostLogRequest(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnEndRequest(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnPostEndRequest(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnSendResponse(IHttpContext* pHttpContext, ISendResponseProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnMapPath(IHttpContext* pHttpContext, IMapPathProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnReadEntity(IHttpContext* pHttpContext, IReadEntityProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnCustomRequestNotification(IHttpContext* pHttpContext, ICustomNotificationProvider* pProvider)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnAsyncCompletion(IHttpContext* pHttpContext, uint dwNotification, BOOL fPostNotification, IHttpEventProvider* pProvider, IHttpCompletionInfo* pCompletionInfo)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }
    public void Dispose()
    {

    }

    public void Destructor()
    {

    }
}
