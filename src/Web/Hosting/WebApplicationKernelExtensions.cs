using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.SemanticKernel;
using System.Runtime.Serialization;

namespace SemanticKernel.Community.WebHosting;
public static class WebApplicationKernelExtensions
{
    private static RequestDelegate WriteAsJsonAsync<T>(T value)
        => context => context.Response.WriteAsJsonAsync(value);

    public static void MapKernel(this IEndpointRouteBuilder app, Kernel kernel, PathString? path = null)
    {
        path ??= $"/{kernel.GetHashCode()}";
        var kernelGroup = app.MapGroup(path);
        kernelGroup.MapGet("/", WriteAsJsonAsync(kernel));
        kernelGroup.MapPost("/", async context =>
        {
            var args = await context.Request.ReadFromJsonAsync<KernelArguments>(context.RequestAborted)
                ?? throw new SerializationException("Failed to deserialize request body.");
            await context.WriteKernelResponseAsync(args, kernel);
        });

        //var servicesGroup = kernelGroup.MapGroup("/services");
        //TODO: Find a way to actually get all registered AI services or remove this functionality.
        // var services = kernel.GetAllServices<IAIService>();
        // var servicesDictionary = services.ToDictionary(service => service.GetType().Name);
        // foreach(var service in services)
        // {
        //     var serviceGroup = servicesGroup.MapGroup($"/{service.GetType().Name}");
        //     serviceGroup.MapGet("/", WriteAsJsonAsync(service));

        //     var serviceInstanceGroup = serviceGroup.MapGroup($"/{service.GetHashCode()}");
        //     serviceInstanceGroup.MapGet("/", WriteAsJsonAsync(service));
        // }
        
        var pluginsGroup = kernelGroup.MapGroup("/plugins");
        var pluginsDictionary = kernel.Plugins.ToDictionary(plugin => plugin.Name);
        pluginsGroup.MapGet("/", WriteAsJsonAsync(pluginsDictionary));
        foreach(var plugin in kernel.Plugins)
        {
            var pluginGroup = pluginsGroup.MapGroup($"/{plugin.Name}");
            pluginGroup.MapGet("/", WriteAsJsonAsync(plugin));

            var functionsGroup = pluginGroup.MapGroup("/functions");
            var functionsDictionary = plugin.ToDictionary(function => function.Name);
            functionsGroup.MapGet("/", WriteAsJsonAsync(functionsDictionary));
            foreach(var function in plugin)
            {
                var functionGroup = functionsGroup.MapGroup($"/{function.Name}");
                functionGroup.MapGet("/", WriteAsJsonAsync(function));
                functionGroup.MapPost("/", async context =>
                {
                    var args = await context.Request.ReadFromJsonAsync<KernelArguments>(context.RequestAborted)
                        ?? throw new SerializationException("Failed to deserialize request body.");
                    var response = await function.InvokeAsync(kernel, args);
                    await context.Response.WriteAsJsonAsync(response);
                });
            }
        }
    }
}