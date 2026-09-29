using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Audacia.UnitTest.Dependency.Customisations;

/// <summary>
/// Customisation for a blueprint that uses a substitute dependency.
/// </summary>
/// <typeparam name="TDependency">The type of the dependency to be customised.</typeparam>
/// <typeparam name="TResult">The type of the result returned by the dependency method.</typeparam>
public class BlueprintCustomisation<TDependency, TResult> : IBlueprintCustomisation<TDependency>
    where TDependency : class
{
    private readonly Func<TDependency, TResult>? _syncCall;
    private readonly Func<TDependency, Task<TResult>>? _asyncCall;
    private readonly TResult? _result;
    private readonly Exception? _exception;
    private readonly bool _isAsync;

    /// <summary>
    /// Creates a synchronous customisation that returns a result.
    /// </summary>
    /// <param name="call">The call to be customised, invoked against the substitute (e.g. <c>x =&gt; x.Method(Arg.Any&lt;int&gt;())</c>).</param>
    /// <param name="result">The result to be returned by the customised method.</param>
    public BlueprintCustomisation(Func<TDependency, TResult> call, TResult result)
    {
        _syncCall = call;
        _result = result;
        _isAsync = false;
    }

    /// <summary>
    /// Creates an asynchronous customisation that returns a result.
    /// </summary>
    /// <param name="call">The call to be customised, invoked against the substitute.</param>
    /// <param name="result">The result to be returned by the customised method.</param>
    public BlueprintCustomisation(Func<TDependency, Task<TResult>> call, TResult result)
    {
        _asyncCall = call;
        _result = result;
        _isAsync = true;
    }

    /// <summary>
    /// Creates a synchronous customisation that throws an exception.
    /// </summary>
    /// <param name="call">The call to be customised, invoked against the substitute.</param>
    /// <param name="exception">The exception to be thrown by the customised method.</param>
    public BlueprintCustomisation(Func<TDependency, TResult> call, Exception exception)
    {
        _syncCall = call;
        _exception = exception;
        _isAsync = false;
    }

    /// <summary>
    /// Creates an asynchronous customisation that throws an exception.
    /// </summary>
    /// <param name="call">The call to be customised, invoked against the substitute.</param>
    /// <param name="exception">The exception to be thrown by the customised method.</param>
    public BlueprintCustomisation(Func<TDependency, Task<TResult>> call, Exception exception)
    {
        _asyncCall = call;
        _exception = exception;
        _isAsync = true;
    }

    /// <summary>
    /// Applies the customisation to the substitute.
    /// </summary>
    /// <param name="substitute">The substitute to apply the customisation to.</param>
    public void Apply(TDependency substitute)
    {
        ArgumentNullException.ThrowIfNull(substitute);

        if (_isAsync)
        {
            var call = _asyncCall!(substitute);

            if (_exception != null)
            {
                call.ThrowsAsync(_exception);
            }
            else
            {
                call.Returns(_result!);
            }
        }
        else
        {
            var call = _syncCall!(substitute);

            if (_exception != null)
            {
                call.Throws(_exception);
            }
            else
            {
                call.Returns(_result!);
            }
        }
    }
}
