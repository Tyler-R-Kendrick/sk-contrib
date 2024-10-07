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
    ///         if(userMessage == "exit") await tokenSource.CancelAsync();
    ///         return new(AuthorRole.User, userMessage);
    ///     });
    ///
    ///     await foreach (var message in chat)
    ///     {
    ///         Console.WriteLine(message);
    ///     }
    /// </code>
    /// </example>
    public static async IAsyncEnumerable<ChatMessageContent> InvokeChatAsync(
        this Kernel kernel,
        Func<ChatMessageContent, CancellationTokenSource, Task<ChatMessageContent>> handler,
        ChatHistory chatHistory,
        IChatCompletionService? chatService = null,
        PromptExecutionSettings? executionSettings = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach(var message in InvokeChatAsync(
            kernel, handler.ToAsyncEnumerable(),
            chatHistory, chatService, executionSettings,
            cancellationToken))
        {
            yield return message;
        }
    }

    private static Func<ChatMessageContent, CancellationTokenSource, Task<IEnumerable<ChatMessageContent>>> ToEnumerable(
        this Func<ChatMessageContent, CancellationTokenSource, Task<ChatMessageContent>> func)
        => async (content, token) => [await func(content, token)];
    private static Func<ChatMessageContent, CancellationTokenSource, IAsyncEnumerable<ChatMessageContent>> ToAsyncEnumerable(
        this Func<ChatMessageContent, CancellationTokenSource, Task<ChatMessageContent>> func)
    {
        // Note: This is a workaround for the lack of generator lambdas.
        async IAsyncEnumerable<ChatMessageContent> Transformer(ChatMessageContent content, CancellationTokenSource token)
        {
            yield return await func(content, token);
        }
        return Transformer;
    }


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
    ///         if(userMessage == "exit") await tokenSource.CancelAsync();
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
        Func<ChatMessageContent, CancellationTokenSource, IAsyncEnumerable<ChatMessageContent>>? handler,
        ChatHistory chatHistory,
        IChatCompletionService? chatService = null,
        PromptExecutionSettings? executionSettings = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        chatService ??= kernel.Services.GetRequiredService<IChatCompletionService>();
        var localHandler = handler ?? ((_, _) => Enumerable.Empty<ChatMessageContent>().ToAsyncEnumerable());
        CancellationTokenSource cancellationTokenSource = new();
        async IAsyncEnumerable<ChatMessageContent> LocalHandler(
            ChatMessageContent content,
            [EnumeratorCancellation] CancellationToken token)
        {
            await foreach(var result in localHandler(content, cancellationTokenSource)
                .WithCancellation(token))
            {
                chatHistory.Add(result);
                yield return result;
            }
        }
        var token = cancellationTokenSource.Token;
        while (!token.IsCancellationRequested)
        {
            token.ThrowIfCancellationRequested();
            var content = await chatService.GetChatMessageContentAsync(
                chatHistory, executionSettings, kernel, token);
            yield return content;

            await foreach (var response in LocalHandler(content, token))
            {
                yield return response;
            }
        }
    }

    public static Task<IReadOnlyList<ChatMessageContent>> InvokeChatCompletionsAsync(this Kernel kernel,
        ChatHistory chatHistory,
        IChatCompletionService? chatService = null,
        PromptExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default)
    {
        chatService ??= kernel.Services.GetRequiredService<IChatCompletionService>();
        return chatService.GetChatMessageContentsAsync(
            chatHistory, executionSettings, kernel, cancellationToken);
    }

    public static Task<ChatMessageContent> InvokeChatCompletionAsync(this Kernel kernel,
        ChatHistory chatHistory,
        IChatCompletionService? chatService = null,
        PromptExecutionSettings? executionSettings = null,
        CancellationToken cancellationToken = default)
    {
        chatService ??= kernel.Services.GetRequiredService<IChatCompletionService>();
        return chatService.GetChatMessageContentAsync(
            chatHistory, executionSettings, kernel, cancellationToken);
    }
}
