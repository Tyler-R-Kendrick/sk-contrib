using Microsoft.SemanticKernel;
using System.Net.WebSockets;

namespace SemanticKernel.Community.WebHosting.Filters;

public class WebHookPromptRenderFilter(WebSocket socket)
    : IPromptRenderFilter
{
    public async Task OnPromptRenderAsync(
        PromptRenderContext context,
        Func<PromptRenderContext, Task> next)
        => await socket.HandleAsync(context, next, context.CancellationToken);
}
