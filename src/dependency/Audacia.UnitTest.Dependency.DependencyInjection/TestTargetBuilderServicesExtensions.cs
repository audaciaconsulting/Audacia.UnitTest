using Microsoft.Extensions.DependencyInjection;

namespace Audacia.UnitTest.Dependency.DependencyInjection;

/// <summary>
/// Extensions that let a <see cref="TestTargetBuilder"/> use a project's own service registration, such as an
/// <c>AddApplication</c> extension method, for a single test.
/// </summary>
public static class TestTargetBuilderServicesExtensions
{
    /// <summary>
    /// Supplies the builder with the services registered by <paramref name="register"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This behaves as a <see cref="ServiceRegistration"/> does, but applies only to this builder and takes
    /// precedence over any registration discovered automatically. Anything the services depend on that
    /// <paramref name="register"/> did not register is built by the builder.
    /// </para>
    /// </remarks>
    /// <param name="builder">The builder to add the services to.</param>
    /// <param name="register">Registers the services, typically by calling a project's own <c>AddX</c> extension method.</param>
    /// <returns>The builder.</returns>
    public static TestTargetBuilder WithServices(
        this TestTargetBuilder builder,
        Action<IServiceCollection> register)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(register);

        return builder.WithDependencySource(new DelegateServiceRegistration(register));
    }

    private sealed class DelegateServiceRegistration(Action<IServiceCollection> register) : ServiceRegistration
    {
        protected override void Register(IServiceCollection services)
        {
            register(services);
        }
    }
}
