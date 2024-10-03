using System.ComponentModel;
using Gremlin.Net.Driver;
using Gremlin.Net.Driver.Exceptions;

namespace SKHelpers.Plugins.Gremlin;

public class GremlinPlugin(GremlinClient client)
{
    [KernelFunction]
    [Description("Executes a Gremlin query.")]
    [return: Description("The result of the Gremlin query.")]
    public async Task<string?> ExecuteQueryAsync(
        [Description("The Gremlin query to execute.")]
        string query)
    {
        try
        {
            var result = await client.SubmitAsync<dynamic>(query);
            return result.ToString();
        }
        catch (ResponseException e)
        {
            return $"Error: {e.Message}";
        }
    }

    [KernelFunction]
    [Description("Adds a vertex to the graph.")]
    [return: Description("The result of the operation.")]
    public async Task<string?> AddVertexAsync(
        [Description("The label of the vertex.")]
        string label,
        [Description("The properties of the vertex.")]
        IDictionary<string, object> properties)
    {
        var query = $"g.addV('{label}')";
        foreach (var property in properties)
        {
            query += $".property('{property.Key}', '{property.Value}')";
        }

        return await ExecuteQueryAsync(query);
    }

    [KernelFunction]
    [Description("Adds an edge between two vertices.")]
    [return: Description("The result of the operation.")]
    public async Task<string?> AddEdgeAsync(
        [Description("The label of the edge.")]
        string label,
        [Description("The ID of the source vertex.")]
        string fromVertexId,
        [Description("The ID of the target vertex.")]
        string toVertexId,
        [Description("The properties of the edge.")]
        IDictionary<string, object> properties)
    {
        var query = $"g.V('{fromVertexId}').addE('{label}').to(g.V('{toVertexId}'))";
        foreach (var property in properties)
        {
            query += $".property('{property.Key}', '{property.Value}')";
        }

        return await ExecuteQueryAsync(query);
    }

    [KernelFunction]
    [Description("Gets a vertex by ID.")]
    [return: Description("The vertex details.")]
    public async Task<string?> GetVertexByIdAsync(
        [Description("The ID of the vertex.")]
        string vertexId)
    {
        var query = $"g.V('{vertexId}')";
        return await ExecuteQueryAsync(query);
    }

    [KernelFunction]
    [Description("Gets all vertices with a specific label.")]
    [return: Description("The list of vertices.")]
    public async Task<string?> GetVerticesByLabelAsync(
        [Description("The label of the vertices.")]
        string label)
    {
        var query = $"g.V().hasLabel('{label}')";
        return await ExecuteQueryAsync(query);
    }
}
