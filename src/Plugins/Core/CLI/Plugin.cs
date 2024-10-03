using System.ComponentModel;
using System.Diagnostics;

namespace SKHelpers.Plugins.CLI;

public class Plugin(TextWriter outputWriter, TextReader inputReader)
{
    [KernelFunction]
    [Description("Executes a cli command.")]
    [return: Description("The exit code of the command.")]
    public async Task<int> ExecuteCommandAsync(
        [Description("The command to execute.")]
        string command,
        [Description("The arguments to pass to the command.")]
        string arguments,
        CancellationToken token = default)
    {
        Process process = new()
        {
            StartInfo = new()
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true,
        };
        void handler(object sender, DataReceivedEventArgs args)
        {
            if (args.Data != null)
            {
                outputWriter.WriteLine(args.Data);
            }
        }
        process.OutputDataReceived += handler;
        process.ErrorDataReceived += handler;

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        var inputWriterTask = Task.Run(async () =>
        {
            using var writer = process.StandardInput;
            while (!process.HasExited)
            {
                var input = await inputReader.ReadToEndAsync(token);
                await writer.WriteLineAsync(input);
            }
        }, token);

        await Task.WhenAll(inputWriterTask, process.WaitForExitAsync(token));

        return process.ExitCode;
    }
}
