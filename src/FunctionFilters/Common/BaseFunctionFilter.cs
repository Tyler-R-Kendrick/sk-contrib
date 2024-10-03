namespace SKHelpers.FunctionFilters.Common;

public class BaseFunctionFilter : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
    {
        await OnFunctionInvokingAsync(context);
        if(await ShouldInvokeAsync(context)) await next(context);
        await OnFunctionInvokedAsync(context);
    }

    protected virtual Task<bool> ShouldInvokeAsync(
        FunctionInvocationContext context)
        => Task.FromResult(true);

    protected virtual Task OnFunctionInvokingAsync(
        FunctionInvocationContext context)
        => Task.CompletedTask;

    protected virtual Task OnFunctionInvokedAsync(
        FunctionInvocationContext context)
        => Task.CompletedTask;
}
