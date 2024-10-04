using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.SemanticKernel.ChatCompletion;

public static class KernelExtensions
{
    /// <example>
    /// <code>
    ///     var chat = kernel.InvokeChatAsync(async (content, tokenSource) =>
    ///     {
    ///         var token = tokenSource.Token;
    ///         var userMessage = await Console.In.ReadLineAsync(token);
    ///         if(userMessage == "exit") await cancellationTokenSource.CancelAsync();
    ///         return new(AuthorRole.User, userMessage);
    ///     });
    ///
    ///     await foreach (var message in chat)
    ///     {
    ///         Console.WriteLine(message);
    ///     }
    /// </code>
    /// </example>
    public static async IAsyncEnumerable<ChatMessageContent> InvokeChatAsync(this Kernel kernel,
        Func<ChatMessageContent, CancellationTokenSource, Task<ChatMessageContent>> handler,
        ChatHistory? chatHistory = null,
        IChatCompletionService? chatService = null,
        PromptExecutionSettings? executionSettings = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach(var message in InvokeChatAsync(
            kernel, handler.ToEnumerable(),
            chatHistory, chatService, executionSettings,
            cancellationToken))
        {
            yield return message;
        }
    }

    private static Func<ChatMessageContent, CancellationTokenSource, Task<IEnumerable<ChatMessageContent>>> ToEnumerable(
        this Func<ChatMessageContent, CancellationTokenSource, Task<ChatMessageContent>> func)
        => async (content, token) => [await func(content, token)];


    /// <summary>
    /// 
    /// </summary>
    /// <param name="kernel"></param>
    /// <param name="handler"></param>
    /// <param name="chatHistory"></param>
    /// <param name="chatService"></param>
    /// <param name="executionSettings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <example>
    /// <code>
    ///     var chat = kernel.InvokeChatAsync(async (content, tokenSource) =>
    ///     {
    ///         var token = tokenSource.Token;
    ///         var userMessage = await Console.In.ReadLineAsync(token);
    ///         if(userMessage == "exit") await cancellationTokenSource.CancelAsync();
    ///         return new[]
    ///         {
    ///             new ChatMessageContent(AuthorRole.User, userMessage),
    ///             new ChatMessageContent(AuthorRole.System, "Hello, how can I help you?")
    ///         };
    ///     });
    ///
    ///     await foreach (var message in chat)
    ///     {
    ///         Console.WriteLine(message);
    ///     }
    /// </code>
    /// </example>
    public static async IAsyncEnumerable<ChatMessageContent> InvokeChatAsync(this Kernel kernel,
        Func<ChatMessageContent, CancellationTokenSource, Task<IEnumerable<ChatMessageContent>>>? handler = null,
        ChatHistory? chatHistory = null,
        IChatCompletionService? chatService = null,
        PromptExecutionSettings? executionSettings = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        chatHistory ??= [];
        chatService ??= kernel.Services.GetRequiredService<IChatCompletionService>();
        var localHandler = handler ?? ((_, _) => Task.FromResult(Enumerable.Empty<ChatMessageContent>()));
        var cancellationTokenSource = new CancellationTokenSource();
        async Task<IEnumerable<ChatMessageContent>> LocalHandler(ChatMessageContent content, CancellationToken token)
        {
            chatHistory.Add(content);
            var results = await localHandler(content, cancellationTokenSource);
            chatHistory.AddRange(results);
            return [..results, content];
        }
        var token = cancellationTokenSource.Token;
        while (!token.IsCancellationRequested)
        {
            token.ThrowIfCancellationRequested();
            var content = await chatService.GetChatMessageContentAsync(
                chatHistory, executionSettings, kernel, token);
            yield return content;

            foreach (var response in await LocalHandler(content, token))
            {
                yield return response;
            }
        }
    }

    public static Task<IReadOnlyList<ChatMessageContent>> InvokeChatCompletionsAsync(this Kernel kernel,
        ChatHistory? chatHistory = null,
        IChatCompletionService? chatService = null,
        PromptExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default)
    {
        chatHistory ??= [];
        chatService ??= kernel.Services.GetRequiredService<IChatCompletionService>();
        return chatService.GetChatMessageContentsAsync(
            chatHistory, executionSettings, kernel, cancellationToken);
    }

    public static Task<ChatMessageContent> InvokeChatCompletionAsync(this Kernel kernel,
        ChatHistory? chatHistory = null,
        IChatCompletionService? chatService = null,
        PromptExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default)
    {
        chatHistory ??= [];
        chatService ??= kernel.Services.GetRequiredService<IChatCompletionService>();
        return chatService.GetChatMessageContentAsync(
            chatHistory, executionSettings, kernel, cancellationToken);
    }
}
