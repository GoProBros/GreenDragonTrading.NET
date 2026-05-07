using GreenDragonTrading.Application.Interfaces;
using StackExchange.Redis;
using System.Globalization;
using System.Text.Json;

namespace GreenDragonTrading.Infrastructure.Services
{
    public sealed record RedisHashFieldsWrite(string Key, Dictionary<string, object> FieldValues);

    public sealed record RedisStringWrite(string Key, object Value, TimeSpan? Expiry = null);

    public sealed record RedisListPushTrimWrite(string Key, object Value, int MaxLength);

    /// <inheritdoc/>
    public class RedisService(IConnectionMultiplexer redis) : IRedisService
    {
        private readonly IConnectionMultiplexer _redis = redis;
        private readonly IDatabase _db = redis.GetDatabase();
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <inheritdoc/>
        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var jsonValue = JsonSerializer.Serialize(value);
            if (expiry.HasValue) return await _db.StringSetAsync(key, jsonValue, expiry.Value);
            return await _db.StringSetAsync(key, jsonValue);
        }

        /// <inheritdoc/>
        public async Task SetHashAsync<T>(string key, T value)
        {
            var entries = ConvertToHashEntries(value);

            await _db.HashSetAsync(key, entries);
        }

        /// <inheritdoc/>
        public async Task<T?> GetAsync<T> (string key)
        {
            var redisValue = await _db.StringGetAsync(key);
            if (redisValue.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(redisValue!);
        }

        /// <inheritdoc/>
        public async Task<T?> GetHashAsync<T>(string key)
        {
            var entries = await _db.HashGetAllAsync(key);

            if (entries.Length == 0) return default;

            return ConvertFromHashEntries<T>(entries);
        }

        /// <inheritdoc/>
        /// Fix #2: Batch operation using Redis pipeline
        public async Task<Dictionary<string, T?>> GetHashBatchAsync<T>(IEnumerable<string> keys)
        {
            var keysList = keys.ToList();
            if (keysList.Count == 0)
                return new Dictionary<string, T?>();

            // Use Redis batch for pipeline execution (all commands sent at once)
            var batch = _db.CreateBatch();
            var tasks = keysList.Select(key => 
                new { Key = key, Task = batch.HashGetAllAsync(key) }
            ).ToList();

            // Execute all commands in pipeline
            batch.Execute();

            // Wait for all results
            await Task.WhenAll(tasks.Select(t => t.Task));

            // Convert results to dictionary
            var results = new Dictionary<string, T?>();
            foreach (var item in tasks)
            {
                var entries = await item.Task;
                if (entries.Length > 0)
                {
                    results[item.Key] = ConvertFromHashEntries<T>(entries);
                }
                else
                {
                    results[item.Key] = default;
                }
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task SetHashFieldAsync<T>(string key, string fieldName, T value)
        {
            string stringValue = value is string s ? s : JsonSerializer.Serialize(value, _jsonOptions);

            await _db.HashSetAsync(key, fieldName, stringValue);
        }

        /// <inheritdoc/>
        public async Task SetHashFieldsAsync(string key, Dictionary<string, object> fieldValues)
        {
            if (fieldValues == null || fieldValues.Count == 0)
                return;

            // Handle legacy keys that were previously stored as string/list/set.
            // If key type is not hash, remove it so hash write can proceed.
            var keyType = await _db.KeyTypeAsync(key);
            if (keyType != RedisType.None && keyType != RedisType.Hash)
            {
                await _db.KeyDeleteAsync(key);
            }

            var hashEntries = fieldValues.Select(kvp =>
            {
                string stringValue = kvp.Value switch
                {
                    string s => s,
                    DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
                    _ => JsonSerializer.Serialize(kvp.Value, _jsonOptions)
                };
                return new HashEntry(kvp.Key, stringValue);
            }).ToArray();

            try
            {
                await _db.HashSetAsync(key, hashEntries);
            }
            catch (RedisServerException ex) when (ex.Message.StartsWith("WRONGTYPE", StringComparison.OrdinalIgnoreCase))
            {
                // Rare race: key type changed between KeyTypeAsync and HashSetAsync.
                await _db.KeyDeleteAsync(key);
                await _db.HashSetAsync(key, hashEntries);
            }
        }

        /// <inheritdoc/>
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
                    // Use InvariantCulture for all numeric primitives so numbers are always
                    // stored with '.' as decimal separator regardless of the server's locale.
                    string stringValue = value is string s
                        ? s
                        : value is IConvertible
                            ? Convert.ToString(value, CultureInfo.InvariantCulture)!
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
                    else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                    {
                        if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var iv))
                            prop.SetValue(obj, iv);
                    }
                    else if (prop.PropertyType == typeof(long) || prop.PropertyType == typeof(long?))
                    {
                        if (long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var lv))
                            prop.SetValue(obj, lv);
                    }
                    else if (prop.PropertyType == typeof(double) || prop.PropertyType == typeof(double?))
                    {
                        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dv))
                            prop.SetValue(obj, dv);
                    }
                    else if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?))
                    {
                        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var mv))
                            prop.SetValue(obj, mv);
                    }
                    else if (prop.PropertyType == typeof(float) || prop.PropertyType == typeof(float?))
                    {
                        if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var fv))
                            prop.SetValue(obj, fv);
                    }
                    else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
                    {
                        if (bool.TryParse(value, out var bv))
                            prop.SetValue(obj, bv);
                    }
                    else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                    {
                        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dtv))
                            prop.SetValue(obj, dtv);
                    }
                    else
                    {
                        // Complex/object types: stored as JSON by ConvertToHashEntries
                        var complexObj = JsonSerializer.Deserialize(value, prop.PropertyType, _jsonOptions);
                        prop.SetValue(obj, complexObj);
                    }
                }
            }
            return obj;
        }

        /// <inheritdoc/>
        public async Task<long> DeleteByPatternAsync(string pattern)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern).ToArray();
            
            if (keys.Length == 0)
            {
                return 0;
            }

            return await _db.KeyDeleteAsync(keys);
        }

        /// <inheritdoc/>
        public async Task<string[]> GetKeysAsync(string pattern)
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern)
                .Select(k => k.ToString())
                .ToArray();
            
            return await Task.FromResult(keys);
        }

        /// <inheritdoc/>
        public async Task ListPushTrimAsync<T>(string key, T value, int maxLength)
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            var batch = _db.CreateBatch();
            var pushTask = batch.ListLeftPushAsync(key, json);
            var trimTask = batch.ListTrimAsync(key, 0, maxLength - 1);
            batch.Execute();
            await Task.WhenAll(pushTask, trimTask);
        }

        /// <inheritdoc/>
        public async Task<List<T>> ListRangeAsync<T>(string key, int count = -1)
        {
            // count <= 0 means "all" — Redis uses -1 as end-index to mean the last element.
            var values = await _db.ListRangeAsync(key, 0, count <= 0 ? -1 : count - 1);
            return values
                .Where(v => !v.IsNullOrEmpty)
                .Select(v => JsonSerializer.Deserialize<T>(v!, _jsonOptions)!)
                .Where(v => v != null)
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<long> ListRightPushAsync<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            return await _db.ListRightPushAsync(key, json);
        }

        /// <inheritdoc/>
        public async Task<T?> ListLeftPopAsync<T>(string key)
        {
            var value = await _db.ListLeftPopAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)value.ToString();
            }

            return JsonSerializer.Deserialize<T>(value.ToString(), _jsonOptions);
        }

        /// <inheritdoc/>
        public async Task<bool> SortedSetAddAsync(string key, string member, double score)
        {
            return await _db.SortedSetAddAsync(key, member, score);
        }

        /// <inheritdoc/>
        public async Task<bool> SortedSetRemoveAsync(string key, string member)
        {
            return await _db.SortedSetRemoveAsync(key, member);
        }

        /// <inheritdoc/>
        public async Task<List<string>> SortedSetRangeByScoreAsync(string key, double start, double stop)
        {
            var values = await _db.SortedSetRangeByScoreAsync(key, start, stop);
            return values
                .Where(v => !v.IsNullOrEmpty)
                .Select(v => v.ToString())
                .ToList();
        }

        /// <summary>
        /// Executes a mixed Redis write batch in a single pipeline flush.
        /// </summary>
        public async Task ExecuteBatchedWritesAsync(
            IReadOnlyCollection<RedisHashFieldsWrite>? hashWrites,
            IReadOnlyCollection<RedisStringWrite>? stringWrites,
            IReadOnlyCollection<RedisListPushTrimWrite>? listWrites)
        {
            if ((hashWrites == null || hashWrites.Count == 0)
                && (stringWrites == null || stringWrites.Count == 0)
                && (listWrites == null || listWrites.Count == 0))
            {
                return;
            }

            var batch = _db.CreateBatch();
            var tasks = new List<Task>();

            if (hashWrites != null)
            {
                foreach (var write in hashWrites)
                {
                    if (write.FieldValues.Count == 0)
                    {
                        continue;
                    }

                    var hashEntries = write.FieldValues.Select(kvp =>
                    {
                        string stringValue = kvp.Value switch
                        {
                            string s => s,
                            DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
                            _ => JsonSerializer.Serialize(kvp.Value, _jsonOptions)
                        };
                        return new HashEntry(kvp.Key, stringValue);
                    }).ToArray();

                    tasks.Add(batch.HashSetAsync(write.Key, hashEntries));
                }
            }

            if (stringWrites != null)
            {
                foreach (var write in stringWrites)
                {
                    var payload = JsonSerializer.Serialize(write.Value, _jsonOptions);
                    if (write.Expiry.HasValue)
                    {
                        tasks.Add(batch.StringSetAsync(write.Key, payload, write.Expiry.Value));
                    }
                    else
                    {
                        tasks.Add(batch.StringSetAsync(write.Key, payload));
                    }
                }
            }

            if (listWrites != null)
            {
                foreach (var write in listWrites)
                {
                    var payload = JsonSerializer.Serialize(write.Value, _jsonOptions);
                    tasks.Add(batch.ListLeftPushAsync(write.Key, payload));
                    tasks.Add(batch.ListTrimAsync(write.Key, 0, write.MaxLength - 1));
                }
            }

            batch.Execute();
            await Task.WhenAll(tasks);
        }
    }
}
