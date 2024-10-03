using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace SKHelpers.Plugins.FileProvider;

public static class KernelBuilderPluginsExtensions
{
    private static T Get<T>(IServiceProvider provider) where T : class => provider.GetRequiredService<T>();
    public static IKernelBuilderPlugins AddFileProvider(this IKernelBuilderPlugins plugins)
    {
        plugins.Services.AddSingleton(provider
            => new Plugin(Get<IFileProvider>(provider)));
        return plugins;
    }
}
