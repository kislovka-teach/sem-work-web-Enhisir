using Microsoft.Extensions.Caching.Memory;
using WebHelper.Models;
using MemoryCache = WebHelper.Auth.MemoryCache;

namespace WebHelper.Tools;

public class Session
{
    public Guid Id { get; } = Guid.NewGuid();
    public AbstractUser? Value { get; set; }

    public void Save() 
        => MemoryCache.Cache.Set(
            Id.ToString(), 
            this, 
            TimeSpan.FromDays(30d));

    public static Session? GetById(string? key)
    {
        if (key is null) return null;
        
        MemoryCache.Cache.TryGetValue(key, out Session? maybeSession);
        return maybeSession;
    }
}