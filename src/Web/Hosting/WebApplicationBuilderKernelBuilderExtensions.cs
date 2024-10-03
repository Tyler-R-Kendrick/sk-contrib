using Microsoft.Extensions.DependencyInjection;

namespace SemanticKernel.Community.Web.Hosting;

public static class WebApplicationBuilderKernelBuilderExtensions
{
    public static IServiceCollection AddSemanticKernel(
        this IServiceCollection services, Action<IKernelBuilder> configure)
        => services.AddTransient(provider =>
        {
            var kernelBuilder = provider.GetService<IKernelBuilder>()
                ?? Kernel.CreateBuilder();
            configure(kernelBuilder);
            return kernelBuilder.Build();
        });
}
