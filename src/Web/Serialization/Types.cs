using System.Net.Http.Json;
using System.Runtime.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace SemanticKernel.Community.Web.Serialization;

public record FilterSocketConfig(string Uri);

public record KernelRecord(
    int Id,
    IDictionary<string, object?> Data,
    Dictionary<string, FilterSocketConfig> Filters,
    Dictionary<string, KernelPluginRecord> Plugins)
{
    public static implicit operator KernelRecord(Kernel target) => new(
        target.GetHashCode(),
        target.Data,
        new Dictionary<string, FilterSocketConfig>
        {
            ["AutoFunctionInvocation"] = new("/filters/auto-function-invocation"),
            ["FunctionInvocation"] = new("/filters/function-invocation"),
            ["PromptRender"] = new("/filters/prompt-render"),
        },
        target.Plugins.ToDictionary(
            plugin => plugin.Name,
            plugin => (KernelPluginRecord)plugin));
}

public record KernelPluginRecord(
    string Name,
    string Description,
    Dictionary<string, KernelFunctionRecord> Functions)
{
    public static implicit operator KernelPluginRecord(
        KernelPlugin target) => new(
            target.Name,
            target.Description,
            target.ToDictionary(
                function => function.Name,
                function => (KernelFunctionRecord)function));
}

public record KernelFunctionRecord(
    KernelFunctionMetadataRecord Metadata,
    IReadOnlyDictionary<string, PromptExecutionSettings>? ExecutionSettings)
{
    public static implicit operator KernelFunctionRecord(
        KernelFunction target) => new(
            target.Metadata,
            target.ExecutionSettings);

    public static implicit operator KernelFunction(KernelFunctionRecord target) =>
        KernelFunctionFactory.CreateFromMethod(
            async (Kernel kernel, KernelArguments arguments, CancellationToken cancellationToken) =>
            {
                var client = kernel.Services.GetRequiredService<HttpClient>();
                var pluginName = target.Metadata.PluginName;
                var functionName = target.Metadata.Name;
                var response = await client.GetAsync(
                    $"plugins/{pluginName}/functions/{functionName}", cancellationToken);
                var function = await response.Content.ReadFromJsonAsync<FunctionResultRecord>(cancellationToken)
                    ?? throw new SerializationException("Failed to deserialize function.");
                return (FunctionResult)function;
            },
            target.Metadata.Name,
            target.Metadata.Description,
            target.Metadata.Parameters.Select(parameter => (KernelParameterMetadata)parameter),
            (KernelReturnParameterMetadata)target.Metadata.ReturnParameter);
}

public record KernelFunctionMetadataRecord(
    string Name,
    string? PluginName,
    string Description,
    IEnumerable<KernelFunctionParameterRecord> Parameters,
    KernelFunctionReturnParameterRecord ReturnParameter,
    IDictionary<string, object?> AdditionalProperties)
{
    public static implicit operator KernelFunctionMetadataRecord(
        KernelFunctionMetadata target) => new(
            target.Name,
            target.PluginName,
            target.Description,
            target.Parameters.Select(parameter => (KernelFunctionParameterRecord)parameter),
            target.ReturnParameter,
            target.AdditionalProperties);
}

public record KernelFunctionParameterRecord(
    string Name,
    string Description,
    string? ParameterType,
    KernelJsonSchema? Schema,
    object? DefaultValue,
    bool IsRequired)
{
    public static implicit operator KernelFunctionParameterRecord(
        KernelParameterMetadata target) => new(
            target.Name,
            target.Description,
            target.ParameterType?.FullName,
            target.Schema,
            target.DefaultValue,
            target.IsRequired);

    public static implicit operator KernelParameterMetadata(
        KernelFunctionParameterRecord target) => new(target.Name)
        {
            Description = target.Description,
            ParameterType = target.ParameterType is null ? null : Type.GetType(target.ParameterType),
            Schema = target.Schema,
            DefaultValue = target.DefaultValue,
            IsRequired = target.IsRequired,
            Name = target.Name,
        };
}

public record KernelFunctionReturnParameterRecord(
    string Description,
    string? ParameterType,
    KernelJsonSchema? Schema)
{
    public static implicit operator KernelFunctionReturnParameterRecord(
        KernelReturnParameterMetadata target) => new(
            target.Description,
            target.ParameterType?.FullName,
            target.Schema);

    public static implicit operator KernelReturnParameterMetadata(
        KernelFunctionReturnParameterRecord target) => new()
        {
            Description = target.Description,
            ParameterType = target.ParameterType is null ? null : Type.GetType(target.ParameterType),
            Schema = target.Schema,
        };
}

public record InvokeArgs(
    IDictionary<string, object?> KernelArguments,
    IReadOnlyDictionary<string, PromptExecutionSettings>? ExecutionSettings)
{
    public static implicit operator KernelArguments(InvokeArgs args) =>
        new(args.KernelArguments, args.ExecutionSettings?.ToDictionary() ?? []);

    public static implicit operator InvokeArgs(KernelArguments args) =>
        new(args, args.ExecutionSettings);
}

public record FunctionResultRecord(
    KernelFunctionRecord Function,
    IReadOnlyDictionary<string, object?>? Metadata,
    string? RenderedPrompt,
    string? ValueType,
    object? Value)
{
    private static object? GetValue(FunctionResult result)
    {
        var methodInfo = typeof(FunctionResult).GetMethod(nameof(result.GetValue));
        var genericMethod = methodInfo?.MakeGenericMethod(result.ValueType ?? typeof(string));
        return genericMethod?.Invoke(result, []);
    }

    public static implicit operator FunctionResultRecord(FunctionResult result) =>
        new(result.Function, result.Metadata, result.RenderedPrompt, result.ValueType?.FullName, GetValue(result));

    public static implicit operator FunctionResult(FunctionResultRecord result) =>
        new(result.Function, result.Metadata);
}
