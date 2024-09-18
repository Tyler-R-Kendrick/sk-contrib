using Microsoft.Extensions.FileProviders;
using Microsoft.SemanticKernel;

namespace SemanticKernel.Community.PromptTemplates.Abstractions;

public interface IPromptTemplateProvider
{
    async Task<PromptTemplateConfig> GetConfigAsync(
        IFileInfo fileInfo,
        string? templateFormat = null,
        CancellationToken cancellationToken = default)
    {
        using var fileStream = fileInfo.CreateReadStream();
        using StreamReader streamReader = new(fileStream);
        var fileContents = await streamReader.ReadToEndAsync(cancellationToken);
        return new()
        {
            Template = fileContents,
            Name = fileInfo.Name,
            TemplateFormat = templateFormat ?? PromptTemplateConfig.SemanticKernelTemplateFormat
        };
    }

    Task<PromptTemplateConfig> GetConfigAsync(
        string resource,
        IFileProvider fileProvider,
        string? templateFormat = null,
        CancellationToken cancellationToken = default)
    {
        var fileInfo = fileProvider.GetFileInfo(resource);
        return GetConfigAsync(fileInfo, templateFormat, cancellationToken);
    }
}

/// <summary>
/// Allows extension of <see cref="IPromptTemplateProvider"/> for "Add" methods.
/// For example, you could create a "AddYaml" method that would add a Yaml prompt template provider.
/// </summary>
public interface IPromptTemplateProviderBuilder
{
    IPromptTemplateProvider Build();
}