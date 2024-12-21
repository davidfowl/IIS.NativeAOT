using System.Runtime.InteropServices;

namespace WebApplication1;

public partial class NativeMethods
{
    static NativeMethods()
    {
        // This is a workaround for the fact that the native library is not in the same directory as the managed assembly.
        NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, (libraryName, assembly, searchPath) =>
        {
            if (libraryName == "IIS.NativeAOT.dll")
            {
                // The entry point is the cloud assembly
                return NativeLibrary.Load(libraryName);
            }

            return 0;
        });
    }

    [LibraryImport("IIS.NativeAOT.dll", EntryPoint = "RegisterCallbacks")]
    public unsafe static partial int RegisterCallbacks(
        delegate* unmanaged<IntPtr, IntPtr, IntPtr, int> requestCallback,
        delegate* unmanaged<IntPtr, IntPtr, uint, int, IntPtr, IntPtr, int> asyncCallback,
        IntPtr context);
}
