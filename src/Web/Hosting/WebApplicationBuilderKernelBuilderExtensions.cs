using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace SemanticKernel.Community.WebHosting;

public static class WebApplicationBuilderKernelBuilderExtensions
{
    public static WebApplicationBuilder UseKernel(this WebApplicationBuilder builder, Kernel kernel)
    {
        builder.Services.AddSingleton(kernel);
        return builder;
    }
    public static WebApplicationBuilder AddSemanticKernel(this WebApplicationBuilder builder, Action<IKernelBuilder> configure)
    {
        builder.Services.AddTransient(provider =>
        {
            var kernelBuilder = provider.GetService<IKernelBuilder>() ?? Kernel.CreateBuilder();
            configure(kernelBuilder);
            return kernelBuilder.Build();
        });
        return builder;
    }
}
