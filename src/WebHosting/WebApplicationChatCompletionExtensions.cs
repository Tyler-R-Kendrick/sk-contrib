// using Microsoft.AspNetCore.Builder;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.SemanticKernel;
// using Microsoft.SemanticKernel.ChatCompletion;
// using System.Collections.Concurrent;
// using System.Runtime.Serialization;

// namespace SemanticKernel.Community.WebHosting;

// public record WebApplicationChatCompletionOptions(
//     PathString? Path,
//     ChatHistory? History,
//     PromptExecutionSettings? ExecutionSettings,
//     Kernel? Kernel);

// public static class WebApplicationChatCompletionExtensions
// {
//     private static readonly PathString _chatPath = new("/chat");
//     private static readonly Lazy<ConcurrentDictionary<string, ChatHistory>> _lazyHistories = new(() => new());
//     /// <summary>
//     /// 
//     /// </summary>
//     /// <param name="app"></param>
//     /// <param name="chat"></param>
//     /// <param name="historyFactory"></param>
//     /// <param name="executionSettings"></param>
//     /// <param name="kernel"></param>
//     /// <returns></returns>
//     /// <exception cref="SerializationException"></exception>
//     /// <exception cref="ArgumentNullException"></exception>
//     /// <example>
//     /// 
//     /// <code>
//     /// ConcurrentDictionary<string, ChatHistory> histories = [];
//     /// app.MapGet(chat, session => histories.GetOrAdd(session, _ => []), executionSettings, kernel);
//     /// </code>
//     /// 
//     /// </example>
//     public static IEndpointConventionBuilder MapGet(
//         this WebApplication app,
//         IChatCompletionService chat,
//         PathString? path = null,
//         Func<string, ChatHistory>? historyFactory = null,
//         PromptExecutionSettings? executionSettings = null,
//         Kernel? kernel = null)
//     {
//         historyFactory ??= session => _lazyHistories.Value.GetOrAdd(session, _ => []);
// #pragma warning disable ASP0018 // Unused route parameter
//         return app.MapGet("/chat/{session}", async context =>
//         {
//             var request = context.Request;
// #pragma warning disable CA2208 // Instantiate argument exceptions correctly
//             var history = historyFactory(request.RouteValues["session"] as string
//                 ?? throw new ArgumentNullException("session"));
// #pragma warning restore CA2208 // Instantiate argument exceptions correctly
//             await HandleResponseAsync(
//                     app.Services,
//                     chat,
//                     context,
//                     history,
//                     executionSettings,
//                     kernel);
//         });
//     }
// #pragma warning restore ASP0018 // Unused route parameter

//     public static IEndpointConventionBuilder MapGet(
//         this WebApplication app,
//         IChatCompletionService chat,
//         PathString? path = null,
//         ChatHistory? history = null,
//         PromptExecutionSettings? executionSettings = null,
//         Kernel? kernel = null)
//     {
//         path ??= _chatPath;
//         history ??= [];
//         return app.MapGet(path, async context =>
//             await HandleResponseAsync(
//                 app.Services,
//                 chat,
//                 context,
//                 history,
//                 executionSettings,
//                 kernel));
//     }

//     private static async Task HandleResponseAsync(
//         IServiceProvider provider,
//         IChatCompletionService chat,
//         HttpContext context,
//         ChatHistory history,
//         PromptExecutionSettings? executionSettings,
//         Kernel? kernel)
//     {
//         var request = context.Request;
//         var response = await chat.GetChatMessageContentAsync(
//             history,
//             executionSettings ?? request.Query.ToPromptExecutionSettings(),
//             kernel ?? provider.GetRequiredService<Kernel>(),
//             context.RequestAborted);
//         await context.Response.WriteAsJsonAsync(response);
//     }
// }
