using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.SemanticKernel.ImageToText;

public static class KernelExtensions
{
    public static Task<TextContent> InvokeImageToTextAsync(
        this Kernel kernel,
        ImageContent content,
        PromptExecutionSettings? executionSettings = null,
        IImageToTextService? imageToTextService = null,
        CancellationToken cancellationToken = default)
    {
        imageToTextService ??= kernel.Services.GetRequiredService<IImageToTextService>();
        return imageToTextService.GetTextContentAsync(content, executionSettings, kernel, cancellationToken);
    }

    public static Task<IReadOnlyList<TextContent>> InvokeImageToTextsAsync(
        this Kernel kernel,
        ImageContent content,
        PromptExecutionSettings? executionSettings = null,
        IImageToTextService? imageToTextService = null,
        CancellationToken cancellationToken = default)
    {
        imageToTextService ??= kernel.Services.GetRequiredService<IImageToTextService>();
        return imageToTextService.GetTextContentsAsync(content, executionSettings, kernel, cancellationToken);
    }
}