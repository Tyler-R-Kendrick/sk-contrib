using Microsoft.SemanticKernel;
using System.Net.WebSockets;

namespace SemanticKernel.Community.WebHosting.Filters;

public class WebHookAutoFunctionInvocationFilter(WebSocket socket)
    : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(
        AutoFunctionInvocationContext context,
        Func<AutoFunctionInvocationContext, Task> next)
        => await socket.HandleAsync(context, next, context.CancellationToken);
}
