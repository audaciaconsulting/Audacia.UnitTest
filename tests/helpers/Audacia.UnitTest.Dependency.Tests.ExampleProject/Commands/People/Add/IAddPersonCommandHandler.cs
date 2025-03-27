using Audacia.Commands;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Add;

/// <summary>
/// Interface for adding a new person.
/// </summary>
public interface IAddPersonCommandHandler : ICommandHandler<AddPersonCommand, AddPersonCommandResultDto>;