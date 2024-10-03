using System.ComponentModel;
using GraphQL;
using GraphQL.Types;
using GraphQL.SystemTextJson;

namespace SKHelpers.Plugins.GraphQL;

public class Plugin(Schema schema)
{
    [KernelFunction]
    [Description("Execute a GraphQL query.")]
    [return: Description("The result of the query.")]
    public Task<string> QueryAsync(
        [Description("The graphql query to execute.")] string query,
        [Description("The root object to execute the query on.")] string root)
        => schema.ExecuteAsync(_ =>
        {
            _.Query = query;
            _.Root = root;
        });
}
