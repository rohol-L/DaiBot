using DaiBot.Core.Interface;
using System.Collections.Concurrent;

namespace DaiBot.Services
{
    public class StorageService : IStorage
    {
        readonly ConcurrentDictionary<string, object?> dictStorage = new();

        public T? Get<T>(string key)
        {
            dictStorage.TryGetValue(key, out var value);
            if (value is T tValue)
            {
                return tValue;
            }
            return default;
        }

        public string? GetString(string key)
        {
            dictStorage.TryGetValue(key, out var value);
            return value?.ToString();
        }

        public void Set<T>(string key, T value)
        {
            if (dictStorage.ContainsKey(key))
            {
                dictStorage[key] = value;
            }
            else
            {
                dictStorage.TryAdd(key, value);
            }
        }
    }
}
