using GreenDragonTrading.Application.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.Services
{
    public class RedisService(IConnectionMultiplexer redis) : IRedisService
    {
        private readonly IDatabase _db = redis.GetDatabase();
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var jsonValue = JsonSerializer.Serialize(value);
            if (expiry.HasValue) return await _db.StringSetAsync(key, jsonValue, expiry.Value);
            return await _db.StringSetAsync(key, jsonValue);
        }

        public async Task SetHashAsync<T>(string key, T value)
        {
            var entries = ConvertToHashEntries(value);

            await _db.HashSetAsync(key, entries);
        }

        public async Task<T?> GetAsync<T> (string key)
        {
            var redisValue = await _db.StringGetAsync(key);
            if (redisValue.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(redisValue!);
        }

        public async Task<T?> GetHashAsync<T>(string key)
        {
            var entries = await _db.HashGetAllAsync(key);

            if (entries.Length == 0) return default;

            return ConvertFromHashEntries<T>(entries);
        }

        public async Task SetHashFieldAsync<T>(string key, string fieldName, T value)
        {
            string stringValue = value is string s ? s : JsonSerializer.Serialize(value, _jsonOptions);

            await _db.HashSetAsync(key, fieldName, stringValue);
        }

        public async Task SetHashFieldsAsync(string key, Dictionary<string, object> fieldValues)
        {
            if (fieldValues == null || fieldValues.Count == 0)
                return;

            var hashEntries = fieldValues.Select(kvp =>
            {
                string stringValue = kvp.Value is string s
                    ? s
                    : JsonSerializer.Serialize(kvp.Value, _jsonOptions);
                return new HashEntry(kvp.Key, stringValue);
            }).ToArray();

            await _db.HashSetAsync(key, hashEntries);
        }

        public async Task<T?> GetHashFieldAsync<T>(string key, string fieldName)
        {
            var value = await _db.HashGetAsync(key, fieldName);
            if (value.IsNullOrEmpty) return default;

            if (typeof(T) == typeof(string)) return (T)(object)value.ToString();

            return JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            return await _db.KeyDeleteAsync(key);
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _db.KeyExistsAsync(key);
        }

        private static HashEntry[] ConvertToHashEntries<T>(T obj)
        {
            var properties = typeof(T).GetProperties();
            var entries = new List<HashEntry>();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);
                if (value != null)
                {
                    string stringValue = value is string || value.GetType().IsPrimitive
                        ? value.ToString()!
                        : JsonSerializer.Serialize(value, _jsonOptions);

                    entries.Add(new HashEntry(prop.Name, stringValue));
                }
            }
            return [.. entries];
        }

        private static T ConvertFromHashEntries<T>(HashEntry[] entries)
        {
            var obj = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties();
            var dict = entries.ToDictionary(e => e.Name.ToString(), e => e.Value.ToString());

            foreach (var prop in properties)
            {
                if (dict.TryGetValue(prop.Name, out var value))
                {
                    if (prop.PropertyType == typeof(string))
                    {
                        prop.SetValue(obj, value);
                    }
                    else if (prop.PropertyType == typeof(int))
                    {
                        prop.SetValue(obj, int.Parse(value));
                    }
                    else if (prop.PropertyType == typeof(bool))
                    {
                        prop.SetValue(obj, bool.Parse(value));
                    }
                    else
                    {
                        var complexObj = JsonSerializer.Deserialize(value, prop.PropertyType, _jsonOptions);
                        prop.SetValue(obj, complexObj);
                    }
                }
            }
            return obj;
        }
    }
}
