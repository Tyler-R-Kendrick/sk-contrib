using System.Net.WebSockets;
using System.Text.Json;

namespace SemanticKernel.Community.Web.Hosting.Filters;

internal static class WebSocketFilterExtensions
{
    public static async Task HandleAsync<TContext>(
        this WebSocket socket,
        TContext context,
        Func<TContext, Task> next,
        CancellationToken cancellationToken)
    {
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(context);
        await socket.SendAsync(jsonBytes, WebSocketMessageType.Text, true, cancellationToken);
        //TODO: Receive the response from the client. Serialize and update the context.
        await next(context);
    }
}
