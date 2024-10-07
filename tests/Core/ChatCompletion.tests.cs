using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.SemanticKernel.ChatCompletion.Tests;

[TestClass]
public class ChatCompletionTests
{
    [TestMethod]
    public async Task InvokeChatAsyncSucceeds()
    {
        const string
            systemPrompt = "You are a helpful AI assistant.",
            agentResponse = "Hello! How can I help you today?",
            exitMessage = "exit";
            
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
        
        using StringReader reader = new(exitMessage);

        async IAsyncEnumerable<ChatMessageContent> HandleChat(ChatMessageContent content, CancellationTokenSource tokenSource)
        {
            var token = tokenSource.Token;
            var userMessage = await reader.ReadLineAsync(token);
            if (userMessage == exitMessage) await tokenSource.CancelAsync();
            yield return new(AuthorRole.User, userMessage);
        }
        var chat = kernel.InvokeChatAsync(HandleChat, history);

        List<ChatMessageContent> responses = [];
        await foreach (var message in chat)
        {
            responses.Add(message);
        }

        Assert.AreEqual(2, responses.Count);
        Assert.AreEqual(agentResponse, responses[0].Content);
        Assert.AreEqual(exitMessage, responses[1].Content);
    }
}