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
    }
}
