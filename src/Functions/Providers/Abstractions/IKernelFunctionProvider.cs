using Microsoft.Extensions.Logging;

namespace SemanticKernel.Community.Functions.Providers.Abstractions;

public interface IKernelFunctionProvider
{
    KernelFunction Get(string pluginName, string functionName);
}

/// <summary>
/// Allows extension of <see cref="IKernelFunctionProvider"/> for "Add" methods.
/// For example, you could create a "AddYaml" method that would add a Yaml kernel function provider.
/// </summary>
public interface IKernelFunctionProviderBuilder
{
    IKernelFunctionProviderBuilder Add(KernelFunction kernelFunction);
    IKernelFunctionProvider Build();
}

public static partial class KernelFunctionProviderBuilderExtensions
{
    public static IKernelFunctionProviderBuilder WithPromptTemplate(
        this IKernelFunctionProviderBuilder builder,
        PromptTemplateConfig config,
        IPromptTemplateFactory? promptTemplateFactory = null,
        ILoggerFactory? loggerFactory = null)
    {
        var factory = promptTemplateFactory ?? new KernelPromptTemplateFactory(loggerFactory);
        var promptTemplate = factory.Create(config);
        return WithPromptTemplate(builder, promptTemplate, config, loggerFactory);
    }

    public static IKernelFunctionProviderBuilder WithPromptTemplate(
        this IKernelFunctionProviderBuilder builder,
        IPromptTemplate template,
        PromptTemplateConfig config,
        ILoggerFactory? loggerFactory = null)
    {
        var kernelFunction = KernelFunctionFactory.CreateFromPrompt(template, config, loggerFactory);
        return builder.Add(kernelFunction);
    }
}
