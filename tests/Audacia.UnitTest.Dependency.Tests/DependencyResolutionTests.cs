using Audacia.UnitTest.Dependency.Exceptions;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace Audacia.UnitTest.Dependency.Tests;

public sealed class DependencyResolutionTests
{
    [Fact]
    public void A_class_the_builder_constructs_is_listed()
    {
        var builder = new TestTargetBuilder();

        builder.Build<Consumer>();

        builder.Resolutions.ShouldContain(new DependencyResolution(typeof(Leaf), ResolutionKind.Class, null, typeof(Leaf)));
        builder.Resolutions.ShouldContain(new DependencyResolution(typeof(Consumer), ResolutionKind.Class, null, typeof(Consumer)));
    }

    [Fact]
    public void A_dependency_supplied_with_With_is_not_listed()
    {
        var builder = new TestTargetBuilder().With(new Leaf());

        builder.Build<Consumer>();

        builder.Resolutions.ShouldNotContain(resolution => resolution.RequestedType == typeof(Leaf));
    }

    [Fact]
    public void A_dependency_supplied_with_an_explicit_blueprint_is_not_listed()
    {
        var builder = new TestTargetBuilder().WithBlueprint(new PartBlueprint());

        builder.Build<PartConsumer>();

        builder.Resolutions.ShouldNotContain(resolution => resolution.RequestedType == typeof(IPart));
    }

    [Fact]
    public void Loggers_are_summarised_rather_than_listed()
    {
        var builder = new TestTargetBuilder();

        builder.Build<LoggingConsumer>();
        var description = builder.DescribeResolutions();

        description.ShouldContain("1 logger(s)");
        description.ShouldNotContain("NullLogger");
    }

    [Fact]
    public void The_description_gives_the_full_name_of_the_concrete_type_used()
    {
        var builder = new TestTargetBuilder();

        builder.Build<Consumer>();

        builder.DescribeResolutions().ShouldContain("Audacia.UnitTest.Dependency.Tests.DependencyResolutionTests.Leaf");
    }

    [Fact]
    public void A_failure_lists_what_had_been_chosen_before_it()
    {
        var builder = new TestTargetBuilder();

        var exception = Should.Throw<TestTargetBuilderException>(builder.Build<NeedsLeafThenMissingPart>);

        exception.Message.ShouldContain("Could not construct a service for IMissingPart");
        exception.Message.ShouldContain("Dependencies chosen automatically so far");
        exception.Message.ShouldContain("Leaf -> constructed: Audacia.UnitTest.Dependency.Tests.DependencyResolutionTests.Leaf");
    }

    [Fact]
    public void A_failure_with_nothing_chosen_yet_does_not_mention_it()
    {
        var builder = new TestTargetBuilder();

        var exception = Should.Throw<TestTargetBuilderException>(builder.Build<NeedsMissingPart>);

        exception.Message.ShouldNotContain("chosen automatically");
    }

    public interface IPart;

    public interface IMissingPart;

    internal sealed class Leaf;

    internal sealed class Consumer(Leaf leaf)
    {
        public Leaf Leaf => leaf;
    }

    internal sealed class Part : IPart;

    internal sealed class PartBlueprint : BlueprintDependency<IPart>
    {
        public override IPart Build()
        {
            return new Part();
        }
    }

    internal sealed class PartConsumer(IPart part)
    {
        public IPart Part => part;
    }

    internal sealed class LoggingConsumer(ILogger<LoggingConsumer> logger)
    {
        public ILogger<LoggingConsumer> Logger => logger;
    }

    internal sealed class NeedsLeafThenMissingPart(Leaf leaf, IMissingPart part)
    {
        public Leaf Leaf => leaf;

        public IMissingPart Part => part;
    }

    internal sealed class NeedsMissingPart(IMissingPart part)
    {
        public IMissingPart Part => part;
    }
}
