using System.Net.Http.Json;
using System.Runtime.Serialization;
using System.Text.Json;

namespace SemanticKernel.Community.Web.Client;
using Serialization;

public static class RemoteKernelBuilderExtensions
{
    public static async Task AddRemotePluginAsync(
        this IKernelBuilderPlugins plugins,
        string pluginName,
        Uri uri)
    {
        HttpClient httpClient = new() { BaseAddress = uri };
        var pluginRecord = await httpClient.GetFromJsonAsync<KernelPluginRecord>($"plugins/{pluginName}")
            ?? throw new SerializationException("Failed to deserialize plugin.");
        List<KernelFunction> functions = [];
        foreach (var (name, function) in pluginRecord.Functions)
        {
            functions.Add(function);
        }
        var plugin = KernelPluginFactory.CreateFromFunctions(
            pluginName,
            pluginRecord.Description,
            functions);
        plugins.Add(plugin);
    }

    public static async Task<KernelFunction> GetFunctionAsync(
        Uri uri,
        string pluginName,
        string functionName)
    {
        HttpClient httpClient = new() { BaseAddress = uri };
        var functionPath = $"plugins/{pluginName}/functions/{functionName}";
        var function = await httpClient.GetFromJsonAsync<KernelFunctionRecord>(functionPath)
            ?? throw new SerializationException("Failed to deserialize function.");
        var metadata = function.Metadata;
        var parameterMetadata = metadata.Parameters;
        var returnParameterMetadata = metadata.ReturnParameter;
        async Task<FunctionResult> Invoke(
            Kernel kernel,
            KernelArguments? arguments = null,
            CancellationToken cancellationToken = default)
        {
            var httpResponse = await httpClient.PostAsJsonAsync(
                functionPath, arguments, cancellationToken: cancellationToken);
            var content = httpResponse.Content;
            var result = await content.ReadFromJsonAsync<FunctionResult>(cancellationToken: cancellationToken);
            return result ?? throw new SerializationException("Failed to deserialize function result.");
        }

        return KernelFunctionFactory.CreateFromMethod(
            method: Invoke,
            functionName: function.Metadata.Name,
            description: function.Metadata.Description,
            parameters: parameterMetadata.Select(x => (KernelParameterMetadata)x),
            returnParameter: returnParameterMetadata
        );
    }

    public static async Task<IKernelBuilderPlugins> AddFromUriAsync(
        this IKernelBuilderPlugins plugins, Uri uri)
    {
        HttpClient httpClient = new() { BaseAddress = uri };
        var remotePluginDictionaryResponse = await httpClient.GetStringAsync("plugins");
        //TODO: Serialize native functions as "remote functions" that just invoke the function at the calculated endpoint.
        //TODO: Possibly use grpc to execute functions remotely.
        var remotePluginDictionary = JsonSerializer.Deserialize<Dictionary<string, KernelPlugin>>(remotePluginDictionaryResponse)
            ?? throw new SerializationException("Failed to deserialize plugin dictionary.");
        var remotePlugins = remotePluginDictionary.Values;
        foreach (var plugin in remotePlugins)
        {
            plugins.Add(plugin);
        }
        return plugins;
    }

    public static async Task<Kernel> BuildKernelAsync(string uri)
    {
        Uri baseUri = new(uri);
        var kernelBuilder = Kernel.CreateBuilder();
        Uri pluginsUri = new(baseUri + "/plugins");
        await kernelBuilder.Plugins.AddFromUriAsync(pluginsUri);
        //TODO: Add Services registered in the remote kernel as RemoteAIService.

        return kernelBuilder.Build();
    }
}
