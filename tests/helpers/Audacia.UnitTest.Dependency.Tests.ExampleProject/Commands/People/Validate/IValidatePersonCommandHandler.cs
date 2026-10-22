using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Validate;

/// <summary>
/// Interface for validating a person, used to test mocking up a real handler.
/// </summary>
public interface IValidatePersonCommandHandler : IRequestHandler<ValidatePersonCommand, CommandResult>;
