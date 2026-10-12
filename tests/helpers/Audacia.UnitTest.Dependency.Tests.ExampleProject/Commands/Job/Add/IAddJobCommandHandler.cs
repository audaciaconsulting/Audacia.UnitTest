using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Job.Add;

/// <summary>
/// Interface for adding a new job.
/// </summary>
public interface IAddJobCommandHandler : IRequestHandler<AddJobCommand, CommandResult<AddJobCommandResultDto>>;