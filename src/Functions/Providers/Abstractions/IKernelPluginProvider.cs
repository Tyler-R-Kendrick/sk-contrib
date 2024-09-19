using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.FileProviders;
using Microsoft.SemanticKernel;

namespace SemanticKernel.Community.Functions.Providers.Abstractions;

public interface IKernelPluginProvider
{
    KernelPlugin Get(string pluginName);
}

public interface IKernelPluginProviderBuilder
{
    IKernelPluginProviderBuilder Add(KernelPlugin kernelPlugin);
    IKernelPluginProvider Build();
}

public static partial class KernelPluginProviderBuilderFileProviderExtensions
{
    public static IKernelPluginProviderBuilder Add(
        this IKernelPluginProviderBuilder builder,
        string pluginName, params KernelFunction[] functions)
    {
        var plugin = KernelPluginFactory.CreateFromFunctions(pluginName, functions);
        return builder.Add(plugin);
    }

    public static async Task<IKernelPluginProviderBuilder> AddDirectoryAsync(
        this IKernelPluginProviderBuilder builder,
        string pluginName,
        string directoryPath,
        IFileProvider fileProvider,
        IPromptTemplateFactory? promptTemplateFactory,
        string? templateFormat,
        ILoggerFactory? loggerFactory,
        CancellationToken cancellationToken = default)
    {
        var directoryContents = fileProvider.GetDirectoryContents(directoryPath);
        var kernelFunctions = new List<KernelFunction>();
        await foreach(var kernelFunction in GetAsync(
            directoryContents,
            promptTemplateFactory,
            templateFormat,
            loggerFactory,
            cancellationToken))
            kernelFunctions.Add(kernelFunction);
        return Add(builder, pluginName, kernelFunctions.ToArray());
    }
    private static async IAsyncEnumerable<KernelFunction> GetAsync(
        IDirectoryContents directoryContents,
        IPromptTemplateFactory promptTemplateFactory,
        string? templateFormat,
        ILoggerFactory? loggerFactory,
        CancellationToken cancellationToken = default)
    {
        foreach (var fileInfo in directoryContents)
        {
            using var fileStream = fileInfo.CreateReadStream();
            using StreamReader streamReader = new(fileStream);
            var fileContents = await streamReader.ReadToEndAsync(cancellationToken);
            PromptTemplateConfig config = new()
            {
                Name = fileInfo.Name,
                Template = fileContents,
                TemplateFormat =  templateFormat ?? PromptTemplateConfig.SemanticKernelTemplateFormat
            };
            var template = promptTemplateFactory.Create(config);
            yield return KernelFunctionFactory.CreateFromPrompt(template, config, loggerFactory);
        }
    }
}
