using Microsoft.SemanticKernel.Memory;

namespace SemanticKernel.Community.PromptFilters.Caching;

public class PromptRenderFilter(
    IMemoryStore cache,
    SearchOptions options)
    : IPromptRenderFilter
{
    public async Task OnPromptRenderAsync(
        PromptRenderContext context,
        Func<PromptRenderContext, Task> next)
    {
        await next(context);
        var prompt = context.RenderedPrompt;
        if(prompt is null) return;

        CancellationToken token = new();
        var result = await cache.GetAsync(
            options.CollectionName,
            prompt,
            options.WithEmbedding,
            token);
        if(result is not null)
        {
            context.Result = new(context.Function, result);
        }
    }
}
