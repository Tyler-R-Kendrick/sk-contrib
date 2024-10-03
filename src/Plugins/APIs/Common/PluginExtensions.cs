using Microsoft.SemanticKernel.Plugins.OpenApi.Extensions;

namespace SKHelpers.Plugins.GitHub;

public static class DependencyInjectionExtensions
{
    public static async Task<Kernel> AddApiPluginAsync(
        this Kernel kernel,
        string key, string value,
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