namespace SKHelpers.FunctionFilters.Common;

public static class CompositeFunctionFilterExtensions
{

    public static void Add(
        this CompositeFunctionFilter filter,
        BaseFunctionFilter next)
        => filter.Add(next);

    public static void Add(
        this CompositeFunctionFilter filter,
        DelegatedFunctionFilter next)
        => filter.Add(next);

    public static void Add(
        this CompositeFunctionFilter filter,
        Func<FunctionInvocationContext, Func<FunctionInvocationContext, Task>, Task> next)
        => filter.Add(next);

    public static void Add(
        this CompositeFunctionFilter filter,
        PreFunctionFilter next)
        => filter.Add(new DelegateFunctionFilter(invoking: next));
    
    public static void Add(
        this CompositeFunctionFilter filter,
        PostFunctionFilter next)
        => filter.Add(new DelegateFunctionFilter(invoked: next));

    public static void Add(
        this CompositeFunctionFilter filter,
        PredicateFunctionFilter next)
        => filter.Add(new DelegateFunctionFilter(shouldInvoke: next));
}