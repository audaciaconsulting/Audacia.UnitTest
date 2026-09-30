using System.Globalization;
using System.Text;

namespace Audacia.UnitTest.Dependency.Helpers;

/// <summary>
/// Turns the dependencies a <see cref="TestTargetBuilder"/> chose automatically into text a person can read.
/// </summary>
internal static class ResolutionDescriber
{
    private const string SubstituteNamespace = "Castle.Proxies";

    /// <summary>
    /// Describes the <paramref name="resolutions"/>, one line each. Loggers and options, which are rarely of interest,
    /// are summarised in a final line rather than listed.
    /// </summary>
    /// <param name="resolutions">The resolutions to describe.</param>
    /// <returns>The description, or an empty string when there is nothing to describe.</returns>
    internal static string Describe(IReadOnlyCollection<DependencyResolution> resolutions)
    {
        var builder = new StringBuilder();

        foreach (var resolution in resolutions.Where(resolution => resolution.Kind is not (ResolutionKind.Logger or ResolutionKind.Options)))
        {
            builder.AppendLine(DescribeResolution(resolution));
        }

        AppendSummary(builder, resolutions);

        return builder.ToString().TrimEnd();
    }

    private static string DescribeResolution(DependencyResolution resolution)
    {
        var requested = GetName(resolution.RequestedType, fullName: false);
        var used = GetName(resolution.ImplementationType, fullName: true);
        var substitute = resolution.ImplementationType.Namespace == SubstituteNamespace ? " (substitute)" : string.Empty;

        return resolution.Kind switch
        {
            ResolutionKind.Blueprint => $"  {requested} -> blueprint: {GetName(resolution.Via!, fullName: true)} => {used}{substitute}",
            ResolutionKind.DependencySource => $"  {requested} -> dependency source: {GetName(resolution.Via!, fullName: true)} => {used}{substitute}",
            ResolutionKind.Interface => $"  {requested} -> implementation found by scanning: {used}",
            ResolutionKind.Class => $"  {requested} -> constructed: {used}",
            ResolutionKind.Options => $"  {requested} -> default options: {used}",
            ResolutionKind.Logger => $"  {requested} -> logger: {used}",
            _ => $"  {requested} -> {used}"
        };
    }

    private static void AppendSummary(StringBuilder builder, IReadOnlyCollection<DependencyResolution> resolutions)
    {
        var loggers = resolutions.Count(resolution => resolution.Kind == ResolutionKind.Logger);
        var options = resolutions.Count(resolution => resolution.Kind == ResolutionKind.Options);

        if (loggers > 0 || options > 0)
        {
            builder.AppendLine(CultureInfo.InvariantCulture, $"  Also: {loggers} logger(s) that discard output, {options} default options");
        }
    }

    private static string GetName(Type type, bool fullName)
    {
        var name = type.Name;
        var backtick = name.IndexOf('`', StringComparison.Ordinal);
        if (backtick >= 0)
        {
            name = name[..backtick];
        }

        if (type.IsGenericType)
        {
            name += $"<{string.Join(", ", type.GetGenericArguments().Select(argument => GetName(argument, fullName)))}>";
        }

        if (!fullName)
        {
            return name;
        }

        var prefix = type.DeclaringType is not null ? GetName(type.DeclaringType, fullName) : type.Namespace;

        return string.IsNullOrEmpty(prefix) ? name : $"{prefix}.{name}";
    }
}
