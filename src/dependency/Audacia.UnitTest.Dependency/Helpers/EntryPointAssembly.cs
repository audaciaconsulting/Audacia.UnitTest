namespace Audacia.UnitTest.Dependency.Helpers;

using System.Reflection;

/// <summary>
/// Helper methods to interact with the project containing the test currently being run.
/// </summary>
internal static class EntryPointAssembly
{
    /// <summary>
    /// Get the name of the project containing the test being run.
    /// </summary>
    /// <returns>The nameof the entry point assembly.</returns>
    public static string GetName()
    {
        return GetDirectoryInfo().Name;
    }

    /// <summary>
    /// Get the full path of the project containing the test being run.
    /// </summary>
    /// <returns>The full path of the project containing the test being run.</returns>
    public static string GetAbsolutePath()
    {
        return GetDirectoryInfo().FullName;
    }

    /// <summary>
    /// Load the entry point assembly of the project containing the test being run.
    /// </summary>
    /// <returns>The loaded assembly.</returns>
    public static Assembly Load()
    {
        var name = GetName();
        return Assembly.Load(name);
    }

    /// <summary>
    /// Get the directory of the project that is currently executing the test.
    /// <br />
    /// HACK: This code is being run in {ProjectName}/bin/{BuildConfiguration}/netX.X, so we need to get the parent
    /// 3 times so we get the {ProjectName}.
    /// <br />
    /// This will break if structure of the output directory changed e.g if we target win-x64 specifically.
    /// </summary>
    private static DirectoryInfo GetDirectoryInfo()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        return Directory.GetParent(currentDirectory)!.Parent!.Parent!;
    }
}