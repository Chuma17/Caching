using StackExchange.Redis;
using System.Text.Json;

namespace Caching.Services
{
    public class CacheService : ICacheService
    {
        private IDatabase _cacheDb;
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IConfiguration _configuration;

        public CacheService(IConfiguration configuration)
        {
            _configuration = configuration;

            string redisHost = _configuration["RedisConfig:Host"];
            int redisPort = int.Parse(_configuration["RedisConfig:Port"]); // Replace with your Redis port
            string redisPassword = _configuration["RedisConfig:Password"];

            ConfigurationOptions configOptions = new ConfigurationOptions
            {
                EndPoints = { $"{redisHost}:{redisPort}" },
                Password = redisPassword,
                AbortOnConnectFail = false, // Change based on your desired behavior
            };

            _redisConnection = ConnectionMultiplexer.Connect(configOptions);
            //var redis = ConnectionMultiplexer.Connect("localhost:6379");
            //var redis = ConnectionMultiplexer.Connect("containers-us-west-196.railway.app:6626");
            _cacheDb = _redisConnection.GetDatabase();
        }
        public T GetData<T>(string key)
        {
            var value = _cacheDb.StringGet(key);
            if (!string.IsNullOrEmpty(value))
            {
                return JsonSerializer.Deserialize<T>(value);
            }
            return default;
        }

        public object RemoveData(string key)
        {
            var exist = _cacheDb.KeyExists(key);
            if (exist)
            {
                return _cacheDb.KeyDelete(key);
            }
            return false;
        }

        public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
        {
            var expiryTime = expirationTime.DateTime.Subtract(DateTime.Now);
            return _cacheDb.StringSet(key, JsonSerializer.Serialize(value), expiryTime);
        }
    }
}
