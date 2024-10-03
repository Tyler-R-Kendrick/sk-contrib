using System.ComponentModel;
using MediatR;

namespace SKHelpers.Plugins.MediatR;

public class Plugin(IMediator mediator,
    Func<object, string> serialize,
    Func<string, string, object> deserialize)
{
    [KernelFunction]
    [Description("Sends a request to the mediator.")]
    [return: Description("The result of the request.")]
    public async Task<string> SendAsync(
        [Description("The type of the request.")]
        string requestType,
        [Description("The serialized request to send.")]
        string request,
        CancellationToken cancellationToken = default)
    {
        var requestCLRType = Type.GetType(requestType)
            ?? throw new ArgumentException($"The type '{requestType}' could not be found.");
        var requestObj = deserialize(requestType, request)
            ?? throw new ArgumentException($"The request could not be deserialized to type '{requestCLRType}'.");
        var result = await mediator.Send(requestObj, cancellationToken)
            ?? throw new InvalidOperationException("The request returned a null result.");
        return serialize(result);
    }

    [KernelFunction]
    [Description("Publishes a notification to the mediator.")]
    public async Task PublishAsync(
        [Description("The type of the notification.")]
        string notificationType,
        [Description("The serialized notification to publish.")]
        string notification,
        CancellationToken cancellationToken = default)
    {
        var notificationCLRType = Type.GetType(notificationType)
            ?? throw new ArgumentException($"The type '{notificationType}' could not be found.");
        var notificationObj = deserialize(notificationType, notification)
            ?? throw new ArgumentException($"The notification could not be deserialized to type '{notificationCLRType}'.");
        await mediator.Publish(notificationObj, cancellationToken);
    }

    [KernelFunction]
    [Description("Creates a stream of responses from the mediator.")]
    [return: Description("The stream of responses.")]
    public IAsyncEnumerable<string> CreateStream(
        [Description("The type of the request.")]
        string requestType,
        [Description("The serialized request to send.")]
        string request,
        CancellationToken cancellationToken = default)
    {
        var requestCLRType = Type.GetType(requestType)
            ?? throw new ArgumentException($"The type '{requestType}' could not be found.");
        var requestObj = deserialize(requestType, request)
            ?? throw new ArgumentException($"The request could not be deserialized to type '{requestCLRType}'.");
        return mediator.CreateStream(requestObj, cancellationToken)
            .Select(x => serialize(x ?? throw new InvalidOperationException("The request returned a null result.")));
    }
}
