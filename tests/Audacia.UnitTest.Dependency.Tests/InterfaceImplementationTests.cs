using Audacia.UnitTest.Dependency.Exceptions;
using Shouldly;

namespace Audacia.UnitTest.Dependency.Tests;

public sealed class InterfaceImplementationTests
{
    [Fact]
    public void An_abstract_class_is_skipped_in_favour_of_a_concrete_implementation()
    {
        var shape = new TestTargetBuilder().Build<IShape>();

        shape.ShouldBeOfType<Square>();
    }

    [Fact]
    public void An_interface_with_only_an_abstract_implementation_is_reported_rather_than_constructed()
    {
        var builder = new TestTargetBuilder();

        var exception = Should.Throw<TestTargetBuilderException>(builder.Build<IAbstractOnly>);

        exception.Service.ShouldBe(nameof(IAbstractOnly));
    }

    public interface IShape;

    public interface IAbstractOnly;

    public abstract class ShapeBase : IShape;

    public sealed class Square : ShapeBase;

    public abstract class AbstractOnlyBase : IAbstractOnly;
}
