using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;

namespace SKHelpers.Plugins.GitHub;

public static class DependencyInjectionExtensions
{
    public static async Task<Kernel> AddIFTTTPluginAsync(
        this Kernel kernel,
        ApiManifestPluginParameters apiManifestPluginParameters,
        CancellationToken cancellationToken = default)
    {
        _ = await kernel.ImportPluginFromApiManifestAsync(
            "github",
            "GitHub/apimanifest.json",
            apiManifestPluginParameters,
            cancellationToken)
            .ConfigureAwait(false);
        return kernel;
    }
}