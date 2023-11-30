using Microsoft.Extensions.Caching.Memory;

namespace WebHelper.Auth;

internal static class MemoryCache
{
    private static readonly Lazy<IMemoryCache> LazyCache = new (
        () => 
            new Microsoft.Extensions.Caching.Memory.MemoryCache(new MemoryCacheOptions()),
        isThreadSafe: true
    );

    public static IMemoryCache Cache => LazyCache.Value;
}

