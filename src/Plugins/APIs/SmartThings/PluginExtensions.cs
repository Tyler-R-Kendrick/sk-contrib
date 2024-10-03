using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;

namespace SKHelpers.Plugins.SmartThings;

public static class DependencyInjectionExtensions
{
    public static async Task<Kernel> AddGitHubPluginAsync(
        this Kernel kernel,
        ApiManifestPluginParameters apiManifestPluginParameters,
        CancellationToken cancellationToken = default)
    {
        _ = await kernel.ImportPluginFromApiManifestAsync(
            "smart-things",
            "SmartThings/apimanifest.json",
            apiManifestPluginParameters,
            cancellationToken)
            .ConfigureAwait(false);
        return kernel;
    }
}