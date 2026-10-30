using Audacia.Mediator;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Job.Add;

/// <summary>
///
/// </summary>
/// <param name="Name"></param>
public record AddJobCommand(string Name) : IRequest<CommandResult<AddJobCommandResultDto>>;