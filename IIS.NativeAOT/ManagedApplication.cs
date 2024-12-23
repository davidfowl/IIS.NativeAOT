using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace IIS.NativeAOT;

internal unsafe class ManagedApplication
{
    private readonly delegate* unmanaged<IntPtr, IntPtr, IntPtr, int> _requestCallback;
    private readonly delegate* unmanaged<IntPtr, IntPtr, uint, int, IntPtr, IntPtr, int> _asyncCallback;
    private readonly IntPtr _context;
    private readonly byte[]? _error;

    public ManagedApplication(byte[] error)
    {
        _requestCallback = &ErrorPage;
        _asyncCallback = &OnAsyncCallback;
        _error = error;
        _context = (IntPtr)GCHandle.Alloc(this);
    }

    public ManagedApplication(
        delegate* unmanaged<IntPtr, IntPtr, IntPtr, int> requestCallback,
        delegate* unmanaged<IntPtr, IntPtr, uint, int, IntPtr, IntPtr, int> asyncCallback,
        IntPtr context
        )
    {
        _requestCallback = requestCallback;
        _asyncCallback = asyncCallback;
        _context = context;
    }

    [UnmanagedCallersOnly]
    private unsafe static int ErrorPage(IntPtr context, IntPtr pHttpContext, IntPtr pModuleInfo)
    {
        var app = (ManagedApplication)((GCHandle)context).Target!;
        var error = app._error!;

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

        fixed (byte* pBody = error)
        {
            chunk.FromMemory.pBuffer = pBody;
            chunk.FromMemory.BufferLength = (uint)error.Length;

            uint bytesSent;
            pResponse->WriteEntityChunks(&chunk, 1, fAsync: false, fMoreData: false, &bytesSent);
        }

        return (int)REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_FINISH_REQUEST;
    }

    [UnmanagedCallersOnly]
    private unsafe static int OnAsyncCallback(IntPtr pContext, IntPtr pHttpContext, uint dwNotification, int fPostNotification, IntPtr pProvider, IntPtr pCompletionInfo)
    {
        return (int)REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }


    public unsafe REQUEST_NOTIFICATION_STATUS OnExecuteRequestHandler(IHttpContext* pHttpContext, IHttpEventProvider* pProvider)
    {
        return (REQUEST_NOTIFICATION_STATUS)_requestCallback(_context, (IntPtr)pHttpContext, (IntPtr)pProvider);
    }

    public unsafe REQUEST_NOTIFICATION_STATUS OnAsyncCompletion(IHttpContext* pHttpContext, uint dwNotification, BOOL fPostNotification, IHttpEventProvider* pProvider, IHttpCompletionInfo* pCompletionInfo)
    {
        return (REQUEST_NOTIFICATION_STATUS)_asyncCallback(_context, (IntPtr)pHttpContext, dwNotification, (int)fPostNotification, (IntPtr)pProvider, (IntPtr)pCompletionInfo);
    }
}
