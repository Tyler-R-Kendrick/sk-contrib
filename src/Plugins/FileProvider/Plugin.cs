using System.ComponentModel;
using Microsoft.Extensions.FileProviders;
using Microsoft.SemanticKernel;

namespace SKHelpers.Plugins.FileProvider;

public class Plugin(IFileProvider fileProvider)
{
    [KernelFunction]
    [Description("Reads the content of a file.")]
    [return: Description("The content of the file.")]
    public Task<string> ReadAsync(
        [Description("The path to the file.")]
        string path)
    {
        var fileInfo = fileProvider.GetFileInfo(path);
        if (!fileInfo.Exists) throw new FileNotFoundException($"File not found: {path}");

        using var stream = fileInfo.CreateReadStream();
        using StreamReader reader = new(stream);
        return reader.ReadToEndAsync();
    }
}
