using Audacia.Commands;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Validate;

/// <summary>
/// Interface for validating a person.
/// </summary>
public interface IValidatePersonCommandHandler : ICommandHandler<ValidatePersonCommand>;