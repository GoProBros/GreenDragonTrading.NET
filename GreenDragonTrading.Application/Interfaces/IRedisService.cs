namespace GreenDragonTrading.Application.Interfaces
{
    public interface IRedisService
    {
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);

        Task SetHashAsync<T>(string key, T value);

        Task<T?> GetAsync<T>(string key);

        Task<T?> GetHashAsync<T>(string key);

        Task SetHashFieldAsync<T>(string key, string fieldName, T value);

        Task SetHashFieldsAsync(string key, Dictionary<string, object> fieldValues);

        Task<T?> GetHashFieldAsync<T>(string key, string fieldName);

        Task<bool> RemoveAsync(string key);

        Task<bool> ExistsAsync(string key);
    }
}
