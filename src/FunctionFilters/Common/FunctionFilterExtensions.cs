using Microsoft.SemanticKernel;

namespace SKHelpers.FunctionFilters.Common;

public static class FunctionFilterExtensions
{
    public static IFunctionInvocationFilter Concat(
        this IFunctionInvocationFilter filter,
        params IFunctionInvocationFilter[] next)
        => new CompositeFunctionFilter([filter, .. next]);

    public static void Add(
        this IFunctionInvocationFilter filter,
        DelegateFunctionFilter next)
        => filter.Add(next);
}
