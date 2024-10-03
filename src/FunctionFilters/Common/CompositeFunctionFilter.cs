using System.Collections;

namespace SKHelpers.FunctionFilters.Common;

public partial class CompositeFunctionFilter : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
    {
        foreach (var filter in _filters)
        {
            await filter.OnFunctionInvocationAsync(context, next);
        }
    }
}

public partial class CompositeFunctionFilter(
    params IFunctionInvocationFilter[] filters)
    : ICollection<IFunctionInvocationFilter>
{
    public CompositeFunctionFilter(params DelegateFunctionFilter[] filters)
        : this(filters as IFunctionInvocationFilter[]) { }
    private readonly List<IFunctionInvocationFilter> _filters = new(filters);

    public int Count => ((ICollection<IFunctionInvocationFilter>)_filters).Count;

    public bool IsReadOnly => ((ICollection<IFunctionInvocationFilter>)_filters).IsReadOnly;

    public void Add(IFunctionInvocationFilter item) => ((ICollection<IFunctionInvocationFilter>)_filters).Add(item);

    public void Clear() => ((ICollection<IFunctionInvocationFilter>)_filters).Clear();

    public bool Contains(IFunctionInvocationFilter item) => ((ICollection<IFunctionInvocationFilter>)_filters).Contains(item);

    public void CopyTo(IFunctionInvocationFilter[] array, int arrayIndex) => ((ICollection<IFunctionInvocationFilter>)_filters).CopyTo(array, arrayIndex);

    public IEnumerator<IFunctionInvocationFilter> GetEnumerator() => ((IEnumerable<IFunctionInvocationFilter>)_filters).GetEnumerator();

    public bool Remove(IFunctionInvocationFilter item) => ((ICollection<IFunctionInvocationFilter>)_filters).Remove(item);

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_filters).GetEnumerator();
}

public partial class CompositeFunctionFilter
{
    public static CompositeFunctionFilter operator +(
        CompositeFunctionFilter filter,
        IFunctionInvocationFilter next)
    {
        filter.Add(next);
        return filter;
    }
    
    public static CompositeFunctionFilter operator +(
        CompositeFunctionFilter filter,
        FunctionFilter next)
    {
        filter.Add(next);
        return filter;
    }
    
    public static CompositeFunctionFilter operator +(
        CompositeFunctionFilter filter,
        PredicateFunctionFilter next)
    {
        filter.Add(new DelegateFunctionFilter(shouldInvoke: next));
        return filter;
    }
    
    public static CompositeFunctionFilter operator +(
        CompositeFunctionFilter filter,
        PreFunctionFilter next)
    {
        filter.Add(new DelegateFunctionFilter(invoking: next));
        return filter;
    }
    
    public static CompositeFunctionFilter operator +(
        CompositeFunctionFilter filter,
        PostFunctionFilter next)
    {
        filter.Add(new DelegateFunctionFilter(invoked: next));
        return filter;
    }
    
    public static CompositeFunctionFilter operator +(
        CompositeFunctionFilter filter,
        (
            PreFunctionFilter? invoking,
            PostFunctionFilter? invoked,
            PredicateFunctionFilter? shouldInvoke
        ) tuple)
    {
        filter.Add(tuple);
        return filter;
    }
    
    public static CompositeFunctionFilter operator +(
        CompositeFunctionFilter filter,
        (
            PreFunctionFilter? invoking,
            PostFunctionFilter? invoked
        ) tuple)
    {
        filter.Add(tuple);
        return filter;
    }

    public static CompositeFunctionFilter operator +(
        CompositeFunctionFilter filter,
        DelegateFunctionFilter next)
    {
        filter.Add(next);
        return filter;
    }
}