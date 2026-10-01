using Audacia.Commands;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Add;

/// <summary>
///
/// </summary>
/// <param name="Name"></param>
public record AddPersonCommand(string Name) : ICommand;