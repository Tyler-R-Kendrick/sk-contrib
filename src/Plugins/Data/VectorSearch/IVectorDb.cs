namespace SKHelpers.Plugins.VectorSearch;

public record Embedding(float[] Vector);
public record VectorSearchResult(string[] Labels, Embedding[] Embeddings, Dictionary<string, object> Data);
public interface IVectorDb
{
    public Task<VectorSearchResult> SearchAsync(
        string index,
        string query,
        int topK,
        string tolerance);
    
    public Task AddAsync(
        string index,
        string[] labels,
        Embedding[] embeddings,
        Dictionary<string, object> data);
}