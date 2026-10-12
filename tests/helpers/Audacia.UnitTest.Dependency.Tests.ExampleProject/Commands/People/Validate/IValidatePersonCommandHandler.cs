using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Validate;

/// <summary>
/// Interface for validating a person.
/// </summary>
public interface IValidatePersonCommandHandler : IRequestHandler<ValidatePersonCommand, CommandResult>;