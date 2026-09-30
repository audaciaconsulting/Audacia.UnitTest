namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Services;

/// <summary>
/// A test target that depends on a service that is only available from the container.
/// </summary>
/// <param name="service">The service.</param>
public sealed class RegisteredOnlyServiceUser(IRegisteredOnlyService service)
{
    /// <summary>
    /// Gets the service.
    /// </summary>
    public IRegisteredOnlyService Service => service;
}
