using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.SemanticKernel.TextGeneration;

public static class KernelExtensions
{
    public static Task<TextContent> InvokeTextContentAsync(
        this Kernel kernel,
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        ITextGenerationService? textGenerationService = null,
        CancellationToken cancellationToken = default)
    {
        textGenerationService ??= kernel.Services.GetRequiredService<ITextGenerationService>();
        return textGenerationService.GetTextContentAsync(prompt, executionSettings, kernel, cancellationToken);
    }

    public static Task<IReadOnlyList<TextContent>> InvokeTextContentsAsync(
        this Kernel kernel,
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        ITextGenerationService? textGenerationService = null,
        CancellationToken cancellationToken = default)
    {
        textGenerationService ??= kernel.Services.GetRequiredService<ITextGenerationService>();
        return textGenerationService.GetTextContentsAsync(prompt, executionSettings, kernel, cancellationToken);
    }
}
