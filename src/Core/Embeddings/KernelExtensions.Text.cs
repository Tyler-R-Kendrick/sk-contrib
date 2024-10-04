using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.SemanticKernel.Embeddings;

public static partial class KernelExtensions
{
    public static async Task<ReadOnlyMemory<float>> InvokeTextEmbeddingAsync(
        this Kernel kernel,
        string data,
        ITextEmbeddingGenerationService? embeddingService = null,
        CancellationToken cancellationToken = default)
    {
        embeddingService ??= kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();
        return await embeddingService.GenerateEmbeddingAsync(data, kernel, cancellationToken);
    }

    public static async IAsyncEnumerable<ReadOnlyMemory<float>> InvokeTextEmbeddingsAsync(
        this Kernel kernel,
        IEnumerable<string> data,
        ITextEmbeddingGenerationService? embeddingService = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        embeddingService ??= kernel.Services.GetRequiredService<ITextEmbeddingGenerationService>();
        var results = await embeddingService.GenerateEmbeddingsAsync(data.ToList(), kernel, cancellationToken);
        foreach (var result in results)
        {
            yield return result;
        }
    }
}