using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.SemanticKernel.TextToAudio;

public static class KernelExtensions
{
    public static Task<AudioContent> InvokeAudioContentAsync(
        this Kernel kernel,
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        ITextToAudioService? textToAudioService = null,
        CancellationToken cancellationToken = default)
    {
        textToAudioService ??= kernel.Services.GetRequiredService<ITextToAudioService>();
        return textToAudioService.GetAudioContentAsync(prompt, executionSettings, kernel, cancellationToken);
    }

    public static Task<IReadOnlyList<AudioContent>> InvokeAudioContentsAsync(
        this Kernel kernel,
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        ITextToAudioService? textToAudioService = null,
        CancellationToken cancellationToken = default)
    {
        textToAudioService ??= kernel.Services.GetRequiredService<ITextToAudioService>();
        return textToAudioService.GetAudioContentsAsync(prompt, executionSettings, kernel, cancellationToken);
    }
}