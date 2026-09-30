using Audacia.UnitTest.Dependency.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Services;

/// <summary>
/// Stands in for a project's own registration extension method, such as <c>AddApplication</c>. Discovered automatically.
/// </summary>
public sealed class ExampleServiceRegistration : ServiceRegistration
{
    /// <inheritdoc />
    protected override void Register(IServiceCollection services)
    {
        services.AddScoped<IRegisteredOnlyService, RegisteredOnlyService>();
    }
}
