using Audacia.Commands;
using Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Job.Add;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Job.Validate;

/// <summary>
///
/// </summary>
/// <param name="Job"></param>
public record ValidateJobCommand(AddJobCommand Job) : ICommand;