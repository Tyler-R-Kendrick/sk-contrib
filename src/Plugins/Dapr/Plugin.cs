using Microsoft.SemanticKernel;
using Dapr.Client;
using System.ComponentModel;

namespace SKHelpers.Plugins.Caching;

public class Plugin(DaprClient client)
{
    [KernelFunction]
    [Description("Publishes a message to a topic in a pubsub component")]
    [return: Description("A task representing the asynchronous operation")]
    public Task Notify(
        [Description("The name of the pubsub component")]
        string pubSubName,
        [Description("The name of the topic")]
        string topicName,
        [Description("The data to publish")]
        string data,
        [Description("The metadata associated with the message")]
        Dictionary<string, string> metadata,
        CancellationToken cancellationToken)
        => client.PublishEventAsync(pubSubName, topicName, data, metadata, cancellationToken);
}
