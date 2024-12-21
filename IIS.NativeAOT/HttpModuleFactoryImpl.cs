using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TerraFX.Interop.Windows;

namespace IIS.NativeAOT;

internal unsafe struct HttpModuleFactoryImpl : IHttpModuleFactory.Interface
{
    private IHttpModuleFactory.Vtbl<HttpModuleFactoryImpl>* lpVtbl;

    private static readonly IHttpModuleFactory.Vtbl<HttpModuleFactoryImpl>* VtblInstance = InitVtblInstance();

    private static IHttpModuleFactory.Vtbl<HttpModuleFactoryImpl>* InitVtblInstance()
    {
        var lpVtbl = (IHttpModuleFactory.Vtbl<HttpModuleFactoryImpl>*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(HttpModuleFactoryImpl), sizeof(IHttpModuleFactory.Vtbl<HttpModuleFactoryImpl>));

        lpVtbl->GetHttpModule = &GetHttpModule;
        lpVtbl->Terminate = &Terminate;

        return lpVtbl;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    private static int GetHttpModule(HttpModuleFactoryImpl* self, CHttpModule** ppModule, IModuleAllocator* pAllocator)
    {
        return self->GetHttpModule(ppModule, pAllocator);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    public static void Terminate(HttpModuleFactoryImpl* self)
    {
        self->Terminate();

        NativeMemory.Free(self);
    }

    public static IHttpModuleFactory* Create()
    {
        var pHttpModuleFactory = (HttpModuleFactoryImpl*)NativeMemory.Alloc((uint)sizeof(HttpModuleFactoryImpl));
        pHttpModuleFactory->lpVtbl = VtblInstance;
        return (IHttpModuleFactory*)pHttpModuleFactory;
    }

    public HRESULT GetHttpModule(CHttpModule** ppModule, IModuleAllocator* pAllocator)
    {
        *ppModule = HttpModuleImpl.Create(pAllocator);

        return 0;
    }

    public void Terminate()
    {
    }
}