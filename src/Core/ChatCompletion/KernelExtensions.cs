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
        await foreach(var message in InvokeChatAsync(kernel, async (content, token) =>
        {
            return await handler(content, token);
        }, chatHistory, chatService, executionSettings, cancellationToken))
        {
            yield return message;
        }
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
        var localHandler = handler ?? ((content, token) => Task.FromResult(Enumerable.Empty<ChatMessageContent>()));
        var cancellationTokenSource = new CancellationTokenSource();
        async Task<IEnumerable<ChatMessageContent>> DefaultHandler(ChatMessageContent content, CancellationToken token)
        {
            await localHandler(content, cancellationTokenSource);
            chatHistory.Add(content);
            var enumerable = Enumerable.Empty<ChatMessageContent>();
            return enumerable;
        }
        var token = cancellationTokenSource.Token;
        while (!token.IsCancellationRequested)
        {
            token.ThrowIfCancellationRequested();
            var content = await chatService.GetChatMessageContentAsync(
                chatHistory, executionSettings, kernel, token);
            yield return content;

            foreach (var response in await DefaultHandler(content, token))
            {
                yield return response;
            }
        }
    }
}
