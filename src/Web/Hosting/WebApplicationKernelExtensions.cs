using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Services;
using SemanticKernel.Community.WebHosting.Filters;
using System.Net.WebSockets;
using System.Runtime.Serialization;

namespace SemanticKernel.Community.WebHosting;

public record FilterSocketConfig(string Uri);
public static class WebApplicationKernelExtensions
{
    private static Delegate WriteKernelAsJsonAsync(Kernel kernel)
        => () => Results.Json(new
        {
            Id = kernel.GetHashCode(),
            //kernel.Culture,
            kernel.Data,
            Filters = new Dictionary<string, FilterSocketConfig>{
                ["AutoFunctionInvocation"] = new("kernels/{kernelId}/filters/auto-function-invocation"),
                ["FunctionInvocation"] = new("kernels/{kernelId}/filters/function-invocation"),
                ["PromptRender"] = new("kernels/{kernelId}/filters/prompt-render"),
            },
            Services = WriteKernelServicesAsJsonAsync(kernel),
            Plugins = WriteKernelPluginsAsJsonAsync(kernel),
        });
    private static Dictionary<string, IAIService> WriteKernelServicesAsJsonAsync(Kernel kernel)
        => kernel.GetAllServices<IAIService>().ToDictionary(x => x.GetType().Name);
    private static dynamic WriteKernelPluginAsJsonAsync(KernelPlugin plugin)
    {
        return new
        {
            plugin.Name,
            plugin.Description,
            Functions = WriteKernelFunctionsAsJsonAsync(plugin),
        };
    }

    private static dynamic WriteKernelPluginsAsJsonAsync(Kernel kernel)
    {
        Dictionary<string, dynamic> serializedPlugins = [];
        foreach(var plugin in kernel.Plugins)
        {
            serializedPlugins.Add(plugin.Name, new
            {
                plugin.Name,
                plugin.Description,
                Functions = WriteKernelFunctionsAsJsonAsync(plugin),
            });
        }
        return serializedPlugins;
    }
    private static dynamic WriteKernelFunctionAsJsonAsync(KernelFunction function)
    {
        return new
        {
            Metadata = new
            {
                function.Metadata.Name,
                function.Metadata.PluginName,
                function.Metadata.Description,
                Parameters = function.Metadata.Parameters.Select(parameter => new
                {
                    parameter.Name,
                    parameter.Description,
                    ParameterType = parameter.ParameterType?.FullName,
                    parameter.Schema,
                }),
                ReturnParameter = new
                {
                    function.Metadata.ReturnParameter.Description,
                    ParameterType = function.Metadata.ReturnParameter.ParameterType?.FullName,
                    function.Metadata.ReturnParameter.Schema,
                },
                function.Metadata.AdditionalProperties,
            },
            function.ExecutionSettings,
        };
    }
    private static Dictionary<string, dynamic> WriteKernelFunctionsAsJsonAsync(KernelPlugin plugin)
    {
        return plugin.ToDictionary(function => function.Name, WriteKernelFunctionAsJsonAsync);
    }

    public static IEndpointRouteBuilder MapKernel(
        this IEndpointRouteBuilder app, Kernel kernel, PathString? path = null)
    {
        path ??= $"/";
        var kernelGroup = app.MapGroup(path);
        kernelGroup
            .MapGet("/", WriteKernelAsJsonAsync(kernel))
            .WithOpenApi();
        kernelGroup
            .MapPost("/", async (KernelArguments args, CancellationToken token) =>
            {
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                var input = args["input"] as string ?? throw new ArgumentNullException("input");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
                return await kernel.InvokePromptAsync(input, args, cancellationToken: token);
            })
            .WithOpenApi();

        kernelGroup
            .MapSocket("/filters/auto-function-invocation", (socket) =>
                kernel.AutoFunctionInvocationFilters.Add(
                    new WebHookAutoFunctionInvocationFilter(socket)))
            .WithName("AutoFunctionInvocation")
            .WithOpenApi();
        kernelGroup
            .MapSocket("/filters/function-invocation", (socket) =>
                kernel.FunctionInvocationFilters.Add(
                    new WebHookFunctionInvocationFilter(socket)))
            .WithName("FunctionInvocation")
            .WithOpenApi();
        kernelGroup
            .MapSocket("/filters/prompt-render", (socket) =>
                kernel.PromptRenderFilters.Add(
                    new WebHookPromptRenderFilter(socket)))
            .WithName("PromptRender")
            .WithOpenApi();
        kernelGroup.MapKernelPlugins(kernel);
        return kernelGroup;
    }

    private static RouteHandlerBuilder MapSocket(
        this IEndpointRouteBuilder app, string path, Action<WebSocket> onSocket)
    {
        Delegate @delegate = async (HttpContext context) =>
        {
            var manager = context.WebSockets;
            if(manager.IsWebSocketRequest)
            {
                var socketTask = await manager.AcceptWebSocketAsync();
                var socket = socketTask;
                onSocket(socket);
                return Results.Accepted();
            }
            return Results.BadRequest("Not a WebSocket request.");
        };
        return app.Map(path, @delegate);
    }

    public static IEndpointRouteBuilder MapKernelPlugins(
        this IEndpointRouteBuilder app, Kernel kernel)
    {
        var pluginsGroup = app.MapGroup("/plugins");
        var pluginsDictionary = kernel.Plugins.ToDictionary(plugin => plugin.Name);
        pluginsGroup
            .MapGet("/", () => WriteKernelPluginsAsJsonAsync(kernel))
            .WithOpenApi();
        foreach (var plugin in kernel.Plugins)
        {
            pluginsGroup.MapKernelPlugin(kernel, plugin);
        }
        return pluginsGroup;
    }

    public static IEndpointRouteBuilder MapKernelPlugin(
        this IEndpointRouteBuilder app, Kernel kernel, KernelPlugin plugin)
    {
        var pluginGroup = app.MapGroup($"/{plugin.Name}");
        pluginGroup
            .MapGet("/", () => WriteKernelPluginAsJsonAsync(plugin))
            .WithOpenApi();
        pluginGroup.MapKernelFunctions(kernel, plugin);
        return pluginGroup;
    }

    public static IEndpointRouteBuilder MapKernelFunctions(
        this IEndpointRouteBuilder app, Kernel kernel, KernelPlugin plugin)
    {
        var functionsGroup = app.MapGroup("/functions");
        functionsGroup
            .MapGet("/", () => WriteKernelFunctionsAsJsonAsync(plugin))
            .WithOpenApi();
        foreach (var function in plugin)
        {
            functionsGroup.MapKernelFunction(kernel, function);
        }
        return functionsGroup;
    }

    public static IEndpointRouteBuilder MapKernelFunction(
        this IEndpointRouteBuilder app, Kernel kernel, KernelFunction function)
    {
        var functionGroup = app.MapGroup($"/{function.Name}");
        functionGroup
            .MapGet("/", () => WriteKernelFunctionAsJsonAsync(function))
            .WithOpenApi();
        functionGroup
            .MapPost("/", async context =>
            {
                var args = await context.Request.ReadFromJsonAsync<KernelArguments>(context.RequestAborted)
                    ?? throw new SerializationException("Failed to deserialize request body.");
                var response = await function.InvokeAsync(kernel, args);
                await context.Response.WriteAsJsonAsync(response);
            })
            .WithOpenApi();
        return functionGroup;
    }
}