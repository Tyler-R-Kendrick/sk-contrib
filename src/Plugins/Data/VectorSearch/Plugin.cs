using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace SKHelpers.Plugins.VectorSearch;

public class VectorSearchPlugin(IVectorDb vectorDb) : IVectorDb
{
    [KernelFunction]
    [Description("Searches the vector database.")]
    [return: Description("The search result containing labels, embeddings, and additional data.")]
    public Task<VectorSearchResult> SearchAsync(
        [Description("The index to search.")]
        string index,
        [Description("The query vector.")]
        string query,
        [Description("The number of results to return.")]
        int topK,
        [Description("The distance metric to use.")]
        string tolerance)
        => vectorDb.SearchAsync(index, query, topK, tolerance);

    [KernelFunction]
    [Description("Adds embeddings to the vector database.")]
    public Task AddAsync(
        [Description("The index to add to.")]
        string index,
        [Description("The labels for the embeddings.")]
        string[] labels,
        [Description("The embeddings to add.")]
        Embedding[] embeddings,
        [Description("Additional data to associate with the embeddings.")]
        Dictionary<string, object> data)
        => vectorDb.AddAsync(index, labels, embeddings, data);
}
