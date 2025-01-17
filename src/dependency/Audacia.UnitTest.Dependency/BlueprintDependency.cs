namespace Audacia.UnitTest.Dependency;

/// <summary>
/// Customisation for dependency <typeparamref name="TDependency"/>.
/// </summary>
/// <typeparam name="TDependency">The type of the dependency which is being customised.</typeparam>
public abstract class BlueprintDependency<TDependency> : IBlueprintDependency<TDependency>
    where TDependency : class
{
    /// <inheritdoc />s
    public TDependency? Dependency { get; protected set; }

    /// <inheritdoc />
    public abstract TDependency Build();
}

/// <summary>
/// Customisation for dependency <typeparamref name="TDependency"/>.
/// </summary>
/// <typeparam name="TDependency">The type of the dependency which is being customised.</typeparam>
/// <typeparam name="TCustomisation">A way of customising the dependency.</typeparam>
public abstract class BlueprintDependency<TDependency, TCustomisation> : IBlueprintDependency<TDependency>
    where TDependency : class
{
    public abstract TCustomisation GetInstanceToCustomise();

    public void Customise(Action<TCustomisation> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var instance = this.GetInstanceToCustomise();

        action(instance);
    }

    public TDependency Dependency { get; }


    public abstract TDependency Build();
}

// public class MockHttpClientBlueprint : Blueprint<IHttpClient, MockApiMessageHandler>
// {
//     public IHttpClient Build()
//     {
//         var mockApiMessageHandler = new MockApiMessageHandler();
//         mockApiMessageHandler.When("fihewionf");
//         return mockApiMessageHandler.ToHttpClient();
//     }
// }