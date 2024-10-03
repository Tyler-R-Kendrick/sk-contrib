using System.Net.WebSockets;

namespace SemanticKernel.Community.Web.Hosting.Filters;

public class WebHookFunctionInvocationFilter(WebSocket socket)
    : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
        => await socket.HandleAsync(context, next, context.CancellationToken);
}
