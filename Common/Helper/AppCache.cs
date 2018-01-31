using MeetingSdk.Wpf;
using System.Collections.Concurrent;

namespace Common.Helper
{
    public class AppCache
    {
        private  static IMeetingWindowManager _windowManager;

        private static readonly ConcurrentDictionary<CacheKey, object> _cache = new ConcurrentDictionary<CacheKey, object>();

        public static void AddOrUpdate(CacheKey key, object value)
        {
            if (key == CacheKey.HostId)
            {
                _windowManager = DependencyResolver.Current.GetService<IMeetingWindowManager>();

                _windowManager.HostId = int.Parse(value.ToString());
            }

            _cache.AddOrUpdate(key, value, (cacheKey, oldValue) =>
            {
                return value;
            });
        }

        public static bool TryRemove(CacheKey key)
        {
            object obj;
            return _cache.TryRemove(key, out obj);
        }

        public static object TryGet(CacheKey key)
        {
            object obj;
            _cache.TryGetValue(key, out obj);
            return obj;
        }
    }
}
