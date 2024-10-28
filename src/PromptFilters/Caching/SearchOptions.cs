using Microsoft.SemanticKernel.Embeddings;

namespace SemanticKernel.Community.PromptFilters.Caching;

public record SearchOptions(
    string CollectionName,
    string RecordIdKey,
    ITextEmbeddingGenerationService Embedding,
    bool WithEmbedding = false);
