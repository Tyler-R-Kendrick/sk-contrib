using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.SemanticKernel.ChatCompletion.Tests;

public class ChatCompletionTests
{
    [Test]
    public async Task InvokeChatAsyncSucceeds()
    {
        // Arrange
        const string
            systemPrompt = "You are a helpful AI assistant.",
            agentResponse = "Hello! How can I help you today?",
            exitMessage = "exit";
        string[] expectations = [agentResponse, exitMessage];
        ChatHistory history = [new(AuthorRole.System, systemPrompt)];

        var kernelBuilder = Kernel.CreateBuilder();
        Mock<IChatCompletionService> moqChat = new();
        kernelBuilder.Services.AddSingleton(moqChat.Object);
        var kernel = kernelBuilder.Build();
        
        moqChat.Setup(x => x.GetChatMessageContentsAsync(
            history,
            It.IsAny<PromptExecutionSettings?>(),
            kernel,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync([new(AuthorRole.Assistant, agentResponse)]);
        
        // Act
        using StringReader reader = new(exitMessage);
        async IAsyncEnumerable<ChatMessageContent> HandleChat(
            ChatMessageContent content,
            CancellationTokenSource tokenSource)
        {
            var token = tokenSource.Token;
            var userMessage = await reader.ReadLineAsync(token);
            if (userMessage == exitMessage) await tokenSource.CancelAsync();
            yield return new(AuthorRole.User, userMessage);
        }

        var chat = kernel.InvokeChatAsync(HandleChat, history);

        // Assert
        List<string?> responses = [];
        await foreach (var message in chat.Select(x => x.Content))
            responses.Add(message);

        Assert.That(responses, Has.Count.EqualTo(2));
        Assert.That(responses, Is.EquivalentTo(expectations));
    }
}