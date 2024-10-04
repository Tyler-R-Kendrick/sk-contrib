using System.Drawing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.SemanticKernel.TextToImage;

public static class KernelExtensions
{
    public static Task<string> InvokeImageGenerationAsync(
        this Kernel kernel,
        string prompt,
        Size size,
        ITextToImageService? textToImageService = null,
        CancellationToken cancellationToken = default)
    {
        textToImageService ??= kernel.Services.GetRequiredService<ITextToImageService>();
        return textToImageService.GenerateImageAsync(prompt, size.Width, size.Height, kernel, cancellationToken);
    }
}