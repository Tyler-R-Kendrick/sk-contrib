using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;

namespace SKHelpers.Plugins.GitHub;

public static class DependencyInjectionExtensions
{
    public static Task<Kernel> AddGitHubPluginAsync(
        this Kernel kernel,
        ApiManifestPluginParameters apiManifestPluginParameters,
        CancellationToken cancellationToken = default)
        => kernel.AddApiPluginAsync(
            "github",
            "GitHub/apimanifest.json",
            apiManifestPluginParameters,
            cancellationToken);
}