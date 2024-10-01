using Microsoft.SemanticKernel;
using System.Net.WebSockets;

namespace SemanticKernel.Community.WebHosting.Filters;

public class WebHookFunctionInvocationFilter(WebSocket socket)
    : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
        => await socket.HandleAsync(context, next, context.CancellationToken);
}
