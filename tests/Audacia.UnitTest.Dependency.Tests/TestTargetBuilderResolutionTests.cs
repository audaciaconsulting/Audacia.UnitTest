using Audacia.UnitTest.Dependency.Exceptions;
using Audacia.UnitTest.Dependency.Http.Blueprints;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Asset.Add;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Configuration;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Repositories;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Reporting;
using Shouldly;

namespace Audacia.UnitTest.Dependency.Tests;

/// <summary>
/// Covers the dependency resolution capabilities added to <see cref="TestTargetBuilder"/>.
/// </summary>
public class TestTargetBuilderResolutionTests
{
    [Fact]
    public void Should_resolve_dependency_from_blueprint_that_implements_the_interface_directly()
    {
        // Arrange
        // IReportFormatter has no exported implementation, so it can only come from a blueprint,
        // and ReportFormatterBlueprint does not derive from BlueprintDependency<T>.

        // Act
        var target = new TestTargetBuilder().Build<ReportGenerator>();

        // Assert
        target.Generate("value").ShouldBe("blueprint:value");
    }

    [Fact]
    public void Should_close_open_generic_implementation_using_the_generic_arguments_of_its_interface()
    {
        // Act
        var target = new TestTargetBuilder().Build<RepositoryConsumer>();

        // Assert
        target.Repository.ShouldBeOfType<EntityRepository<AppConfiguration>>();
        target.Repository.GetEntityType().ShouldBe(typeof(AppConfiguration));
    }

    [Fact]
    public void Should_resolve_named_options_from_an_options_snapshot()
    {
        // Arrange
        var namedOptions = new Dictionary<string, AppConfiguration>
        {
            ["primary"] = new() { RetryCount = 1 },
            ["secondary"] = new() { RetryCount = 2 }
        };

        // Act
        var target = new TestTargetBuilder()
            .WithOptionsSnapshot(namedOptions)
            .Build<AppConfigurationSnapshotConsumer>();

        // Assert
        target.Get("primary").RetryCount.ShouldBe(1);
        target.Get("secondary").RetryCount.ShouldBe(2);
    }

    [Fact]
    public void Should_use_the_explicit_default_for_an_options_snapshot_when_one_is_provided()
    {
        // Arrange
        var namedOptions = new Dictionary<string, AppConfiguration>
        {
            ["primary"] = new() { RetryCount = 1 }
        };
        var fallback = new AppConfiguration { RetryCount = 99 };

        // Act
        var target = new TestTargetBuilder()
            .WithOptionsSnapshot(namedOptions, fallback)
            .Build<AppConfigurationSnapshotConsumer>();

        // Assert
        target.Default.RetryCount.ShouldBe(99);
        target.Get("not-configured").RetryCount.ShouldBe(99);
    }

    [Fact]
    public void Should_throw_exception_when_an_options_snapshot_has_no_named_options()
    {
        // Act
        var target = () => new TestTargetBuilder()
            .WithOptionsSnapshot(new Dictionary<string, AppConfiguration>());

        // Assert
        target.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Should_build_a_target_whose_type_is_only_known_at_runtime()
    {
        // Act
        var target = new TestTargetBuilder().Build(typeof(ReportGenerator));

        // Assert
        target.ShouldBeOfType<ReportGenerator>();
    }

    [Fact]
    public void Should_throw_exception_when_a_blueprint_is_passed_as_a_dependency()
    {
        // Act
        var target = () => new TestTargetBuilder().With(new HttpClientFactoryBlueprint());

        // Assert
        var exception = target.ShouldThrow<TestTargetBuilderException>();
        exception.Message.ShouldContain(nameof(TestTargetBuilder.WithBlueprint));
    }

    [Fact]
    public void Should_include_the_full_dependency_chain_in_the_error_message_when_resolution_fails()
    {
        // Act
        var target = () => new TestTargetBuilder().Build<AddAssetCommandHandler>();

        // Assert
        var exception = target.ShouldThrow<TestTargetBuilderException>();
        exception.Message.ShouldContain("constructing these types:");
        exception.Message.ShouldContain(nameof(AddAssetCommandHandler));
    }
}
