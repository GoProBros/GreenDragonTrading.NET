namespace GreenDragonTrading.Application.Interfaces
{
    /// <summary>
    /// Provides methods for interacting with Redis cache.
    /// </summary>
    public interface IRedisService
    {
        /// <summary>
        /// Sets a value in Redis with optional expiry.
        /// </summary>
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);

        /// <summary>
        /// Sets a hash value in Redis.
        /// </summary>
        Task SetHashAsync<T>(string key, T value);

        /// <summary>
        /// Gets a value from Redis by key.
        /// </summary>
        Task<T?> GetAsync<T>(string key);

        /// <summary>
        /// Gets a hash value from Redis by key.
        /// </summary>
        Task<T?> GetHashAsync<T>(string key);

        /// <summary>
        /// Fix #2: Gets multiple hash values from Redis using pipeline for better performance.
        /// Reduces network round-trips from N separate calls to 1 batch call.
        /// </summary>
        /// <param name="keys">List of Redis keys to fetch</param>
        /// <returns>Dictionary mapping keys to values (null if key doesn't exist)</returns>
        Task<Dictionary<string, T?>> GetHashBatchAsync<T>(IEnumerable<string> keys);

        /// <summary>
        /// Sets a field in a Redis hash.
        /// </summary>
        Task SetHashFieldAsync<T>(string key, string fieldName, T value);

        /// <summary>
        /// Sets multiple fields in a Redis hash.
        /// </summary>
        Task SetHashFieldsAsync(string key, Dictionary<string, object> fieldValues);

        /// <summary>
        /// Gets a field value from a Redis hash.
        /// </summary>
        Task<T?> GetHashFieldAsync<T>(string key, string fieldName);

        /// <summary>
        /// Remove data from Redis by key.
        /// </summary>
        Task<bool> RemoveAsync(string key);

        /// <summary>
        /// Check if a key exists in Redis.
        /// </summary>
        Task<bool> ExistsAsync(string key);

        /// <summary>
        /// Delete all keys matching a pattern (e.g., "OHLCV:FPT:*")
        /// </summary>
        Task<long> DeleteByPatternAsync(string pattern);

        /// <summary>
        /// Get all keys matching a pattern (e.g., "HEATMAP:*")
        /// </summary>
        Task<string[]> GetKeysAsync(string pattern);

        /// <summary>
        /// Prepend a value to a Redis list and trim to maxLength.
        /// Use for maintaining fixed-size recent-items lists.
        /// </summary>
        Task ListPushTrimAsync<T>(string key, T value, int maxLength);

        /// <summary>
        /// Get the first <paramref name="count"/> items from a Redis list.
        /// </summary>
        Task<List<T>> ListRangeAsync<T>(string key, int count);
    }
}
