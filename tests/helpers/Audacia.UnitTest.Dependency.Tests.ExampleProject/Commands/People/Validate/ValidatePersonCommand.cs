using Audacia.Mediator;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Add;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Validate;

/// <summary>
///
/// </summary>
/// <param name="Person"></param>
public record ValidatePersonCommand(AddPersonCommand Person) : IRequest<CommandResult>;