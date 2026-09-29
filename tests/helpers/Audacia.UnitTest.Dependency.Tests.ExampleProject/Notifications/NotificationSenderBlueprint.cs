using Audacia.UnitTest.Dependency.Customisations;
using NSubstitute;

namespace Audacia.UnitTest.Dependency.Tests.ExampleProject.Notifications;

/// <summary>
/// A blueprint that derives from <see cref="CustomisedBlueprintDependency{TDependency}"/>, so the
/// dependency is a substitute with the customisations below applied.
/// </summary>
public sealed class NotificationSenderBlueprint : CustomisedBlueprintDependency<INotificationSender>
{
    /// <summary>
    /// The channel returned by the default blueprint.
    /// </summary>
    public const string DefaultChannel = "email";

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationSenderBlueprint"/> class
    /// where every notification is sent successfully on the <see cref="DefaultChannel"/>.
    /// </summary>
    public NotificationSenderBlueprint()
    {
        Customisations.Add(new BlueprintCustomisation<INotificationSender, string>(
            sender => sender.GetChannel(),
            DefaultChannel));

        Customisations.Add(new BlueprintCustomisation<INotificationSender, bool>(
            sender => sender.SendAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()),
            true));
    }
}
