using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Trianz.Enterprise.Operations
{
    public class RedisCacheHelper
    {
        private readonly IDatabase _db;
        private readonly ConnectionMultiplexer _redis;

        public RedisCacheHelper()
        {
            var connectionString = System.Configuration.ConfigurationManager.AppSettings["AWS:Redis:ConnectionString"] ?? "localhost:6379";
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _db = _redis.GetDatabase();
        }

        public RedisCacheHelper(string connectionString)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _db = _redis.GetDatabase();
        }

        public T Get<T>(string key)
        {
            var value = _db.StringGet(key);
            if (value.IsNullOrEmpty) return default(T);
            return JsonConvert.DeserializeObject<T>(value);
        }

        public void Set<T>(string key, T value, TimeSpan? expiry = null)
        {
            var json = JsonConvert.SerializeObject(value);
            _db.StringSet(key, json, expiry);
        }

        public void Remove(string key)
        {
            _db.KeyDelete(key);
        }

        public bool Exists(string key)
        {
            return _db.KeyExists(key);
        }

        public List<T> GetList<T>(string key)
        {
            var values = _db.ListRange(key);
            if (values.Length == 0) return new List<T>();
            return values.Select(v => JsonConvert.DeserializeObject<T>(v)).ToList();
        }

        public void SetList<T>(string key, List<T> values)
        {
            _db.KeyDelete(key);
            foreach (var item in values)
            {
                _db.ListRightPush(key, JsonConvert.SerializeObject(item));
            }
        }

        public Dictionary<string, T> GetDictionary<T>(string key)
        {
            var hash = _db.HashGetAll(key);
            if (hash.Length == 0) return new Dictionary<string, T>();
            return hash.ToDictionary(
                entry => entry.Name.ToString(),
                entry => JsonConvert.DeserializeObject<T>(entry.Value)
            );
        }

        public void SetDictionary<T>(string key, Dictionary<string, T> values)
        {
            foreach (var kvp in values)
            {
                _db.HashSet(key, kvp.Key, JsonConvert.SerializeObject(kvp.Value));
            }
        }

        public HashSet<T> GetSet<T>(string key)
        {
            var values = _db.SetMembers(key);
            if (values.Length == 0) return new HashSet<T>();
            return new HashSet<T>(values.Select(v => JsonConvert.DeserializeObject<T>(v)));
        }

        public void SetSet<T>(string key, HashSet<T> values)
        {
            _db.KeyDelete(key);
            foreach (var item in values)
            {
                _db.SetAdd(key, JsonConvert.SerializeObject(item));
            }
        }

        public long IncrementCounter(string key, TimeSpan? expiry = null)
        {
            var count = _db.StringIncrement(key);
            if (count == 1 && expiry.HasValue)
            {
                _db.KeyExpire(key, expiry);
            }
            return count;
        }
    }
}
