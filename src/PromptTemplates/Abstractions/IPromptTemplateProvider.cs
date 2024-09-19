using Microsoft.Extensions.FileProviders;
using Microsoft.SemanticKernel;

namespace SemanticKernel.Community.PromptTemplates.Abstractions;

public interface IPromptTemplateProvider
{
    Task<PromptTemplateConfig> GetAsync(
        string resourceName,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Allows extension of <see cref="IPromptTemplateProvider"/> for "Add" methods.
/// For example, you could create a "AddYaml" method that would add a Yaml prompt template provider.
/// </summary>
public interface IPromptTemplateProviderBuilder
{
    IPromptTemplateProviderBuilder Add(PromptTemplateConfig config);
    IPromptTemplateProvider Build();
}

public static partial class PromptTemplateProviderBuilderFileProviderExtensions
{
    public static async Task<IPromptTemplateProviderBuilder> AddFileInfoAsync(
        this IPromptTemplateProviderBuilder builder,
        IFileInfo fileInfo,
        string? templateFormat = null,
        CancellationToken cancellationToken = default)
    {
        using var fileStream = fileInfo.CreateReadStream();
        using StreamReader streamReader = new(fileStream);
        var fileContents = await streamReader.ReadToEndAsync(cancellationToken);
        return builder.Add(new()
        {
            Name = fileInfo.Name,
            Template = fileContents,
            TemplateFormat = templateFormat ?? PromptTemplateConfig.SemanticKernelTemplateFormat
        });
    }

    public static Task<IPromptTemplateProviderBuilder> AddFileNameAsync(
        this IPromptTemplateProviderBuilder builder,
        string resourceName,
        IFileProvider fileProvider,
        string? templateFormat = null,
        CancellationToken cancellationToken = default)
    {
        var fileInfo = fileProvider.GetFileInfo(resourceName);
        return AddFileInfoAsync(builder, fileInfo, templateFormat, cancellationToken);
    }
}
