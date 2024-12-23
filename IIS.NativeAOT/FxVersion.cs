namespace IIS.NativeAOT;

internal sealed class FxVer : IComparable<FxVer>
{
    public int Major { get; }
    public int Minor { get; }
    public int Patch { get; }
    public string? PreRelease { get; }

    private FxVer(int major, int minor, int patch, string? preRelease)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = preRelease;
    }

    public static FxVer Parse(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Version string cannot be null or empty.", nameof(version));
        }

        var parts = version.Split('-', 2);
        var mainParts = parts[0].Split('.') switch
        {
            [var major, var minor, var patch] => (major, minor, patch),
            _ => throw new FormatException($"Invalid version format: {version}")
        };

        if (!int.TryParse(mainParts.major, out var majorNumber) ||
            !int.TryParse(mainParts.minor, out var minorNumber) ||
            !int.TryParse(mainParts.patch, out var patchNumber))
        {
            throw new FormatException($"Invalid version format: {version}");
        }

        var preRelease = parts.Length > 1 ? parts[1] : null;
        return new FxVer(majorNumber, minorNumber, patchNumber, preRelease);
    }

    public static bool TryParse(string version, out FxVer? result)
    {
        try
        {
            result = Parse(version);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    public int CompareTo(FxVer? other)
    {
        if (other == null) return 1;

        return (Major.CompareTo(other.Major),
                Minor.CompareTo(other.Minor),
                Patch.CompareTo(other.Patch),
                ComparePreRelease(PreRelease, other.PreRelease)) switch
        {
            (var major, _, _, _) when major != 0 => major,
            (_, var minor, _, _) when minor != 0 => minor,
            (_, _, var patch, _) when patch != 0 => patch,
            (_, _, _, var preRelease) => preRelease
        };
    }

    private static int ComparePreRelease(string? left, string? right)
    {
        if (left == null && right == null) return 0; // Both stable
        if (left == null) return 1; // Stable > Pre-release
        if (right == null) return -1; // Pre-release < Stable

        return string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString()
    {
        return PreRelease == null ? $"{Major}.{Minor}.{Patch}" : $"{Major}.{Minor}.{Patch}-{PreRelease}";
    }
}
