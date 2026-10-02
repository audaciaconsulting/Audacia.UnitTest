namespace Audacia.UnitTest.Dependency.Tests.MediatorFixtures;

/// <summary>
/// Records what ran in the pipeline, so a test can check how many times each step ran.
/// </summary>
public sealed class PipelineLog
{
    private readonly List<string> _entries = [];

    public IReadOnlyList<string> Entries => _entries;

    public void Add(string entry)
    {
        _entries.Add(entry);
    }
}
