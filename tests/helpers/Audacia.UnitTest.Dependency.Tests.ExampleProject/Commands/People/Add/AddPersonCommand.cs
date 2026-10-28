using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Add;

/// <summary>
///
/// </summary>
/// <param name="Name"></param>
public record AddPersonCommand(string Name) : IRequest<CommandResult<AddPersonCommandResultDto>>;