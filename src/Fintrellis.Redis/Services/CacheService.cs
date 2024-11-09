using Fintrellis.Redis.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Fintrellis.Redis.Services
{
    public class CacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public CacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = _redis.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _db.StringGetAsync(key);

            if (!string.IsNullOrEmpty(value))
            {
                return JsonConvert.DeserializeObject<T>(value!);
            }
            return default;
        }

        public async Task<bool> SetAsync<T>(string key, T value, DateTimeOffset expirationTime)
        {
            TimeSpan expiryTime = expirationTime.DateTime.Subtract(DateTime.UtcNow);
            return await _db.StringSetAsync(key, JsonConvert.SerializeObject(value), expiryTime);
        }
    
        public async Task<bool> RemoveData(string key)
        {
            bool _isKeyExist = await _db.KeyExistsAsync(key);
            if (_isKeyExist == true)
            {
                return await _db.KeyDeleteAsync(key);
            }
            return false;
        }
    }
}
