using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Net.WebSockets;
using System.Runtime.Serialization;

namespace SemanticKernel.Community.Web.Hosting;
using Filters;
using Serialization;

public static class WebApplicationKernelExtensions
{
    public static IEndpointRouteBuilder MapKernel(
        this IEndpointRouteBuilder app, Kernel kernel, PathString? path = null)
    {
        path ??= $"/";
        var kernelGroup = app.MapGroup(path);
        kernelGroup
            .MapGet("/", () => Results.Json<KernelRecord>(kernel))
            .WithOpenApi();
        kernelGroup
            .MapPost("/", async (
                InvokeArgs settings,
                CancellationToken token) =>
            {
                var (args, _) = settings;
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                var input = args["input"] as string ?? throw new ArgumentNullException("input");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
                var response = await kernel.InvokePromptAsync(
                    input,
                    settings,
                    cancellationToken: token);
                return (FunctionResultRecord)response;
            })
            .WithOpenApi();
        kernelGroup.MapSockets(kernel);
        kernelGroup.MapKernelPlugins(kernel);
        return kernelGroup;
    }

    public static IEndpointRouteBuilder MapSockets(
        this IEndpointRouteBuilder kernelGroup, Kernel kernel)
    {
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

        return kernelGroup;
    }

    public static RouteHandlerBuilder MapSocket(
        this IEndpointRouteBuilder app, string path, Action<WebSocket> onSocket)
    {
        /*
        * !Note: Avoid RequestDelegate as there is a known bug.
        * Must cast to Delegate to avoid the bug.
        */
        Delegate @delegate = async (HttpContext context) =>
        {
            var manager = context.WebSockets;
            if (manager.IsWebSocketRequest)
            {
                var socket = await manager.AcceptWebSocketAsync();
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
        pluginsGroup
            .MapGet("/", () => Results.Json(kernel.Plugins.Select(plugin => (KernelPluginRecord)plugin)))
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
            .MapGet("/", () => Results.Json((KernelPluginRecord)plugin))
            .WithOpenApi();
        pluginGroup.MapKernelFunctions(kernel, plugin);
        return pluginGroup;
    }

    public static IEndpointRouteBuilder MapKernelFunctions(
        this IEndpointRouteBuilder app, Kernel kernel, KernelPlugin plugin)
    {
        var functionsGroup = app.MapGroup("/functions");
        functionsGroup
            .MapGet("/", () => Results.Json(plugin.Select(function => (KernelFunctionRecord)function)))
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
            .MapGet("/", () => Results.Json((KernelFunctionRecord)function))
            .WithOpenApi();
        functionGroup
            .MapPost("/", async (HttpContext context, CancellationToken token) =>
            {
                var args = await context.Request.ReadFromJsonAsync<KernelArguments>(context.RequestAborted)
                    ?? throw new SerializationException("Failed to deserialize request body.");
                var response = await function.InvokeAsync(kernel, args, token);
                await context.Response.WriteAsJsonAsync(response, token);
            })
            .WithOpenApi();
        return functionGroup;
    }
}