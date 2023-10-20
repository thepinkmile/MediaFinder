using System.Collections.Concurrent;

namespace MediaFinder_v2.Helpers
{
    public static class ConcurrentDictionaryExtensions
    {
        public static void AddOrUpdate<T1, T2>(this ConcurrentDictionary<T1, T2> dictionary, T1 key, T2 newValue)
            where T1 : notnull
        {
            dictionary.AddOrUpdate(key, newValue, (_, _) => newValue);
        }
    }
}
