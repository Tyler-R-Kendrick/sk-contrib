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
    KernelJsonSchema? Schema)
{
    public static implicit operator KernelFunctionParameterRecord(
        KernelParameterMetadata target) => new(
            target.Name,
            target.Description,
            target.ParameterType?.FullName,
            target.Schema);
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
    string? ValueType)
{
    public static implicit operator FunctionResultRecord(FunctionResult result) =>
        new(result.Function, result.Metadata, result.RenderedPrompt, result.ValueType?.FullName);
}