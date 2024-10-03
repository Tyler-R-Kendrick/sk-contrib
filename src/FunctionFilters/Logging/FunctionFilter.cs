using Microsoft.Extensions.Logging;

namespace SKHelpers.FunctionFilters.Logging;

using SKHelpers.FunctionFilters.Common;

public abstract class FunctionFilter(ILogger logger)
    : BaseFunctionFilter
{
    protected abstract Task OnFunctionInvokingAsync(
        FunctionInvocationContext context, ILogger logger);

    protected abstract Task OnFunctionInvokedAsync(
        FunctionInvocationContext context, ILogger logger);

    protected sealed override Task OnFunctionInvokingAsync(
        FunctionInvocationContext context)
        => OnFunctionInvokingAsync(context, logger);

    protected sealed override Task OnFunctionInvokedAsync(
        FunctionInvocationContext context)
        => OnFunctionInvokedAsync(context, logger);
}
