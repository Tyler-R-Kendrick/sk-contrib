using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.SemanticKernel.AudioToText;

public static class KernelExtensions
{
    public static Task<TextContent> InvokeAudioToTextAsync(
        this Kernel kernel,
        AudioContent content,
        PromptExecutionSettings? executionSettings = null,
        IAudioToTextService? audioToTextService = null,
        CancellationToken cancellationToken = default)
    {
        audioToTextService ??= kernel.Services.GetRequiredService<IAudioToTextService>();
        return audioToTextService.GetTextContentAsync(content, executionSettings, kernel, cancellationToken);
    }

    public static Task<IReadOnlyList<TextContent>> InvokeAudioToTextsAsync(
        this Kernel kernel,
        AudioContent content,
        PromptExecutionSettings? executionSettings = null,
        IAudioToTextService? audioToTextService = null,
        CancellationToken cancellationToken = default)
    {
        audioToTextService ??= kernel.Services.GetRequiredService<IAudioToTextService>();
        return audioToTextService.GetTextContentsAsync(content, executionSettings, kernel, cancellationToken);
    }
}