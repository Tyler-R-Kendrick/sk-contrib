using Microsoft.SemanticKernel;

namespace SKHelpers.FunctionFilters.Common;

public class DelegatedFunctionFilter(FunctionFilter? filter = null)
    : IFunctionInvocationFilter
{
    public Task OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
        => filter?.Invoke(context, next) ?? next(context);
    
    public static implicit operator DelegatedFunctionFilter(
        FunctionFilter filter)
        => new(filter);
}
