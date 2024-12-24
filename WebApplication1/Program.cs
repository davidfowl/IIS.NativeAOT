using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;
using WebApplication1;

Console.WriteLine("Hello Managed code!");

unsafe
{
    var handler = new RequestHandler();
    var ret = NativeMethods.RegisterCallbacks(&RequestHandler.OnExecuteRequest, &RequestHandler.OnAsyncCompletion, (IntPtr)GCHandle.Alloc(handler));

    if (ret < 0)
    {
        Console.WriteLine("Failed to register callbacks");
        return;
    }
}

// Wait for ctrl + C or some other signal
var tcs = new TaskCompletionSource();
PosixSignalRegistration.Create(PosixSignal.SIGTERM, x => tcs.TrySetResult());
await tcs.Task;

sealed class RequestHandler
{
    [UnmanagedCallersOnly]
    public static unsafe int OnExecuteRequest(IntPtr context, IntPtr pHttpContext, IntPtr pProvider)
    {
        return (int)((RequestHandler)GCHandle.FromIntPtr(context).Target!).OnExecuteRequest(pHttpContext, pProvider);
    }

    [UnmanagedCallersOnly]
    public static unsafe int OnAsyncCompletion(IntPtr context, IntPtr pHttpContext, uint dwNotification, int fPostNotification, IntPtr pProvider, IntPtr pCompletionInfo)
    {
        return (int)((RequestHandler)GCHandle.FromIntPtr(context).Target!).OnAsyncCompletion(pHttpContext, dwNotification, fPostNotification, pProvider, pCompletionInfo);
    }

    public unsafe REQUEST_NOTIFICATION_STATUS OnExecuteRequest(IntPtr pHttpContext, IntPtr pProvider)
    {
        var httpContext = (IHttpContext*)pHttpContext;

        var pResponse = httpContext->GetResponse();

        var body =
                """
            <html>
               <head>
                <title>Welcome to .NET!</title>
               </head>
               <body>
               <h1>Managed code is running</h1>
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

        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }

    public REQUEST_NOTIFICATION_STATUS OnAsyncCompletion(nint pHttpContext, uint dwNotification, int fPostNotification, nint pProvider, nint pCompletionInfo)
    {
        return REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
    }
}