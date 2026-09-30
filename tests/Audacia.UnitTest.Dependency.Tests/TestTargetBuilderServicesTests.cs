using Audacia.UnitTest.Dependency.DependencyInjection;
using Audacia.UnitTest.Dependency.Exceptions;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Services;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Audacia.UnitTest.Dependency.Tests;

public sealed class TestTargetBuilderServicesTests
{
    [Fact]
    public void A_registered_service_can_be_built()
    {
        var greeter = new TestTargetBuilder()
            .WithServices(services => services.AddScoped<IGreeter, Greeter>())
            .With<IName>(new FixedName("Joe"))
            .Build<IGreeter>();

        greeter.Greet().ShouldBe("Hello Joe");
    }

    [Fact]
    public void A_dependency_that_was_not_registered_comes_from_the_builder_whichever_order_it_is_added_in()
    {
        var before = new TestTargetBuilder()
            .With<IName>(new FixedName("Before"))
            .WithServices(services => services.AddScoped<IGreeter, Greeter>())
            .Build<IGreeter>();

        var after = new TestTargetBuilder()
            .WithServices(services => services.AddScoped<IGreeter, Greeter>())
            .With<IName>(new FixedName("After"))
            .Build<IGreeter>();

        before.Greet().ShouldBe("Hello Before");
        after.Greet().ShouldBe("Hello After");
    }

    [Fact]
    public void A_dependency_that_was_not_registered_can_come_from_a_blueprint()
    {
        var greeter = new TestTargetBuilder()
            .WithServices(services => services.AddScoped<IGreeter, Greeter>())
            .WithBlueprint(new NameBlueprint())
            .Build<IGreeter>();

        greeter.Greet().ShouldBe("Hello Blueprint");
    }

    [Fact]
    public void A_registered_service_wins_over_the_builder_for_its_own_type()
    {
        var greeter = new TestTargetBuilder()
            .WithServices(services => services.AddSingleton<IName>(new FixedName("Registered")).AddScoped<IGreeter, Greeter>())
            .Build<IGreeter>();

        greeter.Greet().ShouldBe("Hello Registered");
    }

    [Fact]
    public void A_service_can_be_injected_into_another_test_target()
    {
        var target = new TestTargetBuilder()
            .WithServices(services => services.AddScoped<IGreeter, Greeter>())
            .With<IName>(new FixedName("Joe"))
            .Build<GreeterUser>();

        target.Greeter.Greet().ShouldBe("Hello Joe");
    }

    [Fact]
    public void A_service_from_a_discovered_registration_needs_no_setup_in_the_test()
    {
        var target = new TestTargetBuilder()
            .With<IUnregisteredPart>(new FixedPart("part"))
            .Build<RegisteredOnlyServiceUser>();

        target.Service.Describe().ShouldBe("Built with part");
    }

    [Fact]
    public void A_dependency_of_a_discovered_registration_that_was_not_registered_can_come_from_a_later_with()
    {
        var target = new TestTargetBuilder();
        target.With<IUnregisteredPart>(new FixedPart("late"));

        target.Build<IRegisteredOnlyService>().Describe().ShouldBe("Built with late");
    }

    [Fact]
    public void A_dependency_that_nothing_can_supply_is_reported_rather_than_looping()
    {
        var builder = new TestTargetBuilder()
            .WithServices(services => services.AddScoped<INeedsMissingPart, NeedsMissingPart>());

        var exception = Should.Throw<TestTargetBuilderException>(builder.Build<INeedsMissingPart>);

        exception.Service.ShouldBe(nameof(IMissingPart));
    }

    [Fact]
    public void A_circular_dependency_between_a_registered_service_and_the_builder_is_reported_rather_than_hanging()
    {
        var builder = new TestTargetBuilder()
            .WithServices(services => services.AddScoped<IRingStart, RingStart>());

        var exception = Should.Throw<TestTargetBuilderException>(builder.Build<IRingStart>);

        exception.Message.ShouldContain("circular");
    }

    [Fact]
    public void A_dependency_of_the_constructor_the_container_uses_comes_from_the_builder_when_a_class_has_several()
    {
        var greeter = new TestTargetBuilder()
            .WithServices(services => services.AddScoped<IGreeter, MultipleConstructorGreeter>())
            .With<IName>(new FixedName("Joe"))
            .Build<IGreeter>();

        greeter.Greet().ShouldBe("Hello Joe");
    }

    [Fact]
    public void A_registration_is_required()
    {
        var builder = new TestTargetBuilder();

        Should.Throw<ArgumentNullException>(() => builder.WithServices(null!));
    }

    public interface IName
    {
        string Value { get; }
    }

    public interface IMissingPart;

    public interface INeedsMissingPart;

    public interface IGreeter
    {
        string Greet();
    }

    public interface IRingStart;

    public interface IRingEnd;

    public sealed class RingStart(IRingEnd end) : IRingStart
    {
        public IRingEnd End => end;
    }

    public sealed class RingEnd(IRingStart start) : IRingEnd
    {
        public IRingStart Start => start;
    }

    internal sealed class MultipleConstructorGreeter : IGreeter
    {
        private readonly IName? _name;

        public MultipleConstructorGreeter()
        {
        }

        public MultipleConstructorGreeter(IName name)
        {
            _name = name;
        }

        public string Greet()
        {
            return $"Hello {_name?.Value}";
        }
    }

    internal sealed class NeedsMissingPart(IMissingPart part) : INeedsMissingPart
    {
        public IMissingPart Part => part;
    }

    internal sealed class FixedName(string value) : IName
    {
        public string Value => value;
    }

    internal sealed class FixedPart(string name) : IUnregisteredPart
    {
        public string Name => name;
    }

    internal sealed class NameBlueprint : BlueprintDependency<IName>
    {
        public override IName Build()
        {
            return new FixedName("Blueprint");
        }
    }

    internal sealed class Greeter(IName name) : IGreeter
    {
        public string Greet()
        {
            return $"Hello {name.Value}";
        }
    }

    public sealed class GreeterUser(IGreeter greeter)
    {
        public IGreeter Greeter => greeter;
    }
}
