using System.Runtime.InteropServices;

namespace IIS.NativeAOT;

// See https://github.com/dotnet/runtime/blob/main/docs/design/features/native-hosting.md for more information on native hosting in .NET.
internal static class HostFxr
{
    public static (string? Error, int? ErrorCode, nint Handle) Initialize(string? dll)
    {
        if (!OperatingSystem.IsWindows())
        {
            return ($"{RuntimeInformation.RuntimeIdentifier} is unsupported.", null, 0);
        }

        var dotnetRoot = GetDotnetRootPath();

        if (dotnetRoot is null)
        {
            return ("Unable to find .NET installation.", null, 0);
        }

        var allHostFxrDirs = new DirectoryInfo(Path.Combine(dotnetRoot, "host", "fxr"));

        // REVIEW: Should we parse the versions and pick the latest properly?
        var hostFxrDirectory = (from d in allHostFxrDirs.EnumerateDirectories()
                                let version = FxVer.Parse(d.Name)
                                orderby version descending
                                select d)
                                .FirstOrDefault();

        if (hostFxrDirectory is null)
        {
            return ($"Unable to find hostfxr in {dotnetRoot}", null, 0);
        }

        // Console.WriteLine($"Using hostfxr: {hostFxrDirectory}");

        NativeLibrary.Load(Path.Combine(hostFxrDirectory.FullName, "hostfxr.dll"));

        if (string.IsNullOrEmpty(dll))
        {
            return ("DOTNET_DLL environment variable is not set.", null, 0);
        }

        string[] args = [dll];

        unsafe
        {
            fixed (char* hostPathPointer = Environment.CurrentDirectory)
            fixed (char* dotnetRootPointer = dotnetRoot)
            {
                var parameters = new HostFxrImports.hostfxr_initialize_parameters
                {
                    size = sizeof(HostFxrImports.hostfxr_initialize_parameters),
                    host_path = hostPathPointer,
                    dotnet_root = dotnetRootPointer
                };

                var err = HostFxrImports.Initialize(args.Length, args, ref parameters, out var host_context_handle);

                if (err < 0)
                {
                    return ($"Error initializing hostfxr {err}", err, 0);
                }

                return (null, null, host_context_handle);
            }
        }
    }

    private static string? GetDotnetRootPath()
    {
        // Check the DOTNET_ROOT environment variable
        var dotnetRootEnv = Environment.GetEnvironmentVariable("DOTNET_ROOT");
        if (!string.IsNullOrEmpty(dotnetRootEnv) && Directory.Exists(dotnetRootEnv))
        {
            return dotnetRootEnv;
        }

        // Check the default installation path for .NET
        var programFilesDotnet = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet");
        if (Directory.Exists(programFilesDotnet))
        {
            return programFilesDotnet;
        }

        // Search for dotnet.exe in the PATH environment variable
        var pathEnvironmentVariable = Environment.GetEnvironmentVariable("PATH");
        if (pathEnvironmentVariable is null)
        {
            return null;
        }

        foreach (string path in pathEnvironmentVariable.Split(Path.PathSeparator))
        {
            string potentialDotnetPath = Path.Combine(path, "dotnet");
            if (File.Exists(Path.Combine(potentialDotnetPath, "dotnet.exe")))
            {
                return potentialDotnetPath;
            }
        }

        return null;
    }
}
