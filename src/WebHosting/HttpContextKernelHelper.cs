using System.Runtime.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.SemanticKernel;

namespace SemanticKernel.Community.WebHosting;

public static class HttpContextKernelHelper
{
    public static PromptExecutionSettings ToPromptExecutionSettings(this IQueryCollection query)
    {
        System.Text.Json.Nodes.JsonObject element = [];
        foreach (var key in query.Keys)
        {
            var value = query[key].ToString();
            element.Add(key, value);
        }
        var settings = JsonSerializer.Deserialize<PromptExecutionSettings>(element.ToString())
            ?? throw new SerializationException("Failed to deserialize request parameters.");
        return settings;
    }

    public static KernelArguments ToKernelArguments(this IQueryCollection query)
    {
        KernelArguments args = [];
        foreach (var key in query.Keys)
        {
            args.Add(key, query[key]);
        }
        return args;
    }

    public static async Task WriteKernelResponseAsync(
        this HttpContext context,
        KernelArguments kernelArguments,
        Kernel kernel)
    {
        var token = context.RequestAborted;
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
        var input = kernelArguments["input"] as string ?? throw new ArgumentNullException("input");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
        var result = await kernel.InvokePromptAsync(input, kernelArguments, cancellationToken: token);
        await context.Response.WriteAsJsonAsync(result, token);
    }
}
