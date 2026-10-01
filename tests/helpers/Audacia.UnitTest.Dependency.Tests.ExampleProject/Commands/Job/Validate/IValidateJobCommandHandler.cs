using Audacia.Commands;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Job.Validate;

/// <summary>
/// This should have no inherit type as it's used to test the <see cref="TestTargetBuilder"/> throws an exception when
/// no type found to create that inherits.
/// </summary>
public interface IValidateJobCommandHandler : ICommandHandler<ValidateJobCommand>;