using Audacia.Commands;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Job.Add;

public interface IAddJobCommandHandler : ICommandHandler<AddJobCommand, AddJobCommandResultDto>
{
}