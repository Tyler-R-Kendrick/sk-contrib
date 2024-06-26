using Microsoft.SemanticKernel;

namespace SKHelpers.FunctionFilters.Common;
public delegate Task FunctionFilter(
    FunctionInvocationContext context,
    Func<FunctionInvocationContext, Task> next);
public delegate Task PreFunctionFilter(
    FunctionInvocationContext context);
public delegate Task<bool> PredicateFunctionFilter(
    FunctionInvocationContext context);
public delegate Task PostFunctionFilter(
    FunctionInvocationContext context);
