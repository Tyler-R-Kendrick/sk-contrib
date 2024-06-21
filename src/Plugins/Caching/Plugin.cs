using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace SKHelpers.Plugins.Caching;

public class Plugin(IMemoryCache memoryCache, Func<string, Task<object>> factory)
{
    [KernelFunction]
    [Description("Gets the value associated with the specified key.")]
    [return: Description("The value associated with the specified key.")]
    public Task<T> GetOrSetAsync<T>(
        [Description("The key of the value to get or set.")]
        string key,
        [Description("The absolute expiration time for the value.")]
        TimeSpan? absoluteExpiration = null,
        [Description("The sliding expiration time for the value.")]
        TimeSpan? slidingExpiration = null)
        => memoryCache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = absoluteExpiration;
            entry.SlidingExpiration = slidingExpiration;
            var result = await factory(key).ConfigureAwait(false);
            return result is T t
                ? t : throw new InvalidCastException($"Cannot cast {result.GetType()} to {typeof(T)}");
        });
}
