using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;
using WebApplication1;

Console.WriteLine("Hello Managed code!");

unsafe
{
    NativeMethods.RegisterCallbacksManual(&RequestHandler);
}

// Wait for ctrl + C or some other signal
var tcs = new TaskCompletionSource();
PosixSignalRegistration.Create(PosixSignal.SIGTERM, x => tcs.TrySetResult());
await tcs.Task;

[UnmanagedCallersOnly]
static unsafe int RequestHandler(IntPtr context, IntPtr _)
{
    var httpContext = (IHttpContext*)context;

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

    return (int)REQUEST_NOTIFICATION_STATUS.RQ_NOTIFICATION_CONTINUE;
}