## IIS.NativeAOT

This is an experiment to see it to build a Native IIS module using Native AOT instead of C++. ASP.NET Core has the ASP.NET Core Module which is a [sizable chunk of C++ code](https://github.com/dotnet/aspnetcore/tree/main/src/Servers/IIS/AspNetCoreModuleV2). This is an attempt to see if we can build a similar module using C# and Native AOT.