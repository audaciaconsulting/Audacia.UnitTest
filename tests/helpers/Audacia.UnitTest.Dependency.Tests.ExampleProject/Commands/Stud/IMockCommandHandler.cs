using Audacia.Commands;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Commands.Stud;

public interface IMockCommandHandler : ICommandHandler<MockCommand, MockCommandResultDto>;