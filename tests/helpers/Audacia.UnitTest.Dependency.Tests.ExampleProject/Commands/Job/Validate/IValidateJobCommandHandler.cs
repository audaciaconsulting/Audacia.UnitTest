using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Job.Validate;

/// <summary>
/// This is used when setting up the handler 'manually', for examples that don't use Audacia.UnitTest.Dependency.Mediator.
/// It should have no inherit type as it's used to test the <see cref="TestTargetBuilder"/> throws an exception when
/// no type found to create that inherits.
/// </summary>
public interface IValidateJobCommandHandler : IRequestHandler<ValidateJobCommand, CommandResult>;