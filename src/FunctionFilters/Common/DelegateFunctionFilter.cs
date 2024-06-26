using Microsoft.SemanticKernel;

namespace SKHelpers.FunctionFilters.Common;

public class DelegateFunctionFilter(
    PreFunctionFilter? invoking = null,
    PostFunctionFilter? invoked = null,
    PredicateFunctionFilter? shouldInvoke = null)
    : BaseFunctionFilter
{
    protected sealed override Task OnFunctionInvokingAsync(
        FunctionInvocationContext context)
        => invoking?.Invoke(context) ?? base.ShouldInvokeAsync(context);

    protected sealed override Task OnFunctionInvokedAsync(
        FunctionInvocationContext context)
        => invoked?.Invoke(context) ?? base.ShouldInvokeAsync(context);

    protected sealed override Task<bool> ShouldInvokeAsync(
        FunctionInvocationContext context)
        => shouldInvoke?.Invoke(context) ?? base.ShouldInvokeAsync(context);

    public static implicit operator DelegateFunctionFilter(
        (
            PreFunctionFilter? invoking,
            PostFunctionFilter? invoked,
            PredicateFunctionFilter? shouldInvoke
        ) tuple)
        => new(tuple.invoking, tuple.invoked, tuple.shouldInvoke);

    public static implicit operator DelegateFunctionFilter(
        (
            PreFunctionFilter? invoking,
            PostFunctionFilter? invoked
        ) tuple)
        => new(tuple.invoking, tuple.invoked);
}
