using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace SKHelpers.Plugins.Logging;

public class LoggingPlugin(ILogger<LoggingPlugin> logger)
{
    [KernelFunction]
    [Description("Logs an information message.")]
    public void LogInformation(
        [Description("The information message to log.")]
        string message)
        => logger.LogInformation(message);

    [KernelFunction]
    [Description("Logs a warning message.")]
    public void LogWarning(
        [Description("The warning message to log.")]
        string message)
        => logger.LogWarning(message);

    [KernelFunction]
    [Description("Logs an error message.")]
    public void LogError(
        [Description("The error message to log.")]
        string message)
        => logger.LogError(message);

    [KernelFunction]
    [Description("Logs a critical error message.")]
    public void LogCritical(
        [Description("The critical error message to log.")]
        string message)
        => logger.LogCritical(message);

    [KernelFunction]
    [Description("Logs a debug message.")]
    public void LogDebug(
        [Description("The debug message to log.")]
        string message)
        => logger.LogDebug(message);

    [KernelFunction]
    [Description("Logs a trace message.")]
    public void LogTrace(
        [Description("The trace message to log.")]
        string message)
        => logger.LogTrace(message);
}
