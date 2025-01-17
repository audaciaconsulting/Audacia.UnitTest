using Audacia.Commands;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.People.Add;

public interface IAddPersonCommandHandler : ICommandHandler<AddPersonCommand, AddPersonCommandResultDto>
{
}