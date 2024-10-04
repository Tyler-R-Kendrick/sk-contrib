using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.SemanticKernel.Embeddings;

public static partial class KernelExtensions
{
    public static async Task<ReadOnlyMemory<TEmbedding>> InvokeEmbeddingAsync<TValue, TEmbedding>(
        this Kernel kernel,
        TValue data,
        IEmbeddingGenerationService<TValue, TEmbedding>? embeddingService = null,
        CancellationToken cancellationToken = default)
        where TEmbedding : unmanaged
    {
        embeddingService ??= kernel.Services.GetRequiredService<IEmbeddingGenerationService<TValue, TEmbedding>>();
        return await embeddingService.GenerateEmbeddingAsync(data, kernel, cancellationToken);
    }

    public static async IAsyncEnumerable<ReadOnlyMemory<TEmbedding>> InvokeEmbeddingsAsync<TValue, TEmbedding>(
        this Kernel kernel,
        IEnumerable<TValue> data,
        IEmbeddingGenerationService<TValue, TEmbedding>? embeddingService = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TEmbedding : unmanaged
    {
        embeddingService ??= kernel.Services.GetRequiredService<IEmbeddingGenerationService<TValue, TEmbedding>>();
        var results = await embeddingService.GenerateEmbeddingsAsync(data.ToList(), kernel, cancellationToken);
        foreach (var result in results)
        {
            yield return result;
        }
    }
}