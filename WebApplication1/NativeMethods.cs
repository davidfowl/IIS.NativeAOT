using System.Runtime.InteropServices;

namespace WebApplication1;

public partial class NativeMethods
{
    // LibraryImport isn't working but manual resolution using NativeLibrary.Load and TryGetExport works

    [LibraryImport("IIS.NativeAOT.dll", EntryPoint = "RegisterCallbacks")]
    public unsafe static partial int RegisterCallbacks(
        delegate* unmanaged<IntPtr, IntPtr, IntPtr, int> requestCallback,
        delegate* unmanaged<IntPtr, IntPtr, uint, int, IntPtr, IntPtr, int> asyncCallback,
        IntPtr context);

    public unsafe static int RegisterCallbacksManual(
        delegate* unmanaged<IntPtr, IntPtr, IntPtr, int> requestCallback,
        delegate* unmanaged<IntPtr, IntPtr, uint, int, IntPtr, IntPtr, int> asyncCallback,
        IntPtr context)
    {
        var iisAot = NativeLibrary.Load("IIS.NativeAOT.dll");

        if (iisAot == 0)
        {
            throw new DllNotFoundException("Unable to find IIS.NativeAOT.dll");
        }

        if (!NativeLibrary.TryGetExport(iisAot, "RegisterCallbacks", out var registerCallbacks))
        {
            throw new EntryPointNotFoundException("Unable to find RegisterCallbacks export");
        }

        return ((delegate* unmanaged<
            delegate* unmanaged<IntPtr, IntPtr, IntPtr, int>,
            delegate* unmanaged<IntPtr, IntPtr, uint, int, IntPtr, IntPtr, int>,
            IntPtr,
            int>)registerCallbacks)(
            requestCallback,
            asyncCallback,
            context);
    }
}
