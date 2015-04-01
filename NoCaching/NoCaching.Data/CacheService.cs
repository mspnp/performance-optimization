// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.Azure;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NoCaching.Data
{
    public class CacheService
    {
        private static ConnectionMultiplexer _connection;

        private const double DefaultExpirationTimeInMinutes = 5.0d;

        private static ConnectionMultiplexer Connection
        {
            get
            {
                if ((_connection == null) || (!_connection.IsConnected))
                {
                    _connection = ConnectionMultiplexer.Connect(CloudConfigurationManager.GetSetting("RedisConfiguration"));
                }

                return _connection;
            }
        }

        public static Task<T> GetAsync<T>(string key, Func<Task<T>> loadCache)
        {
            return GetAsync<T>(key, loadCache, DefaultExpirationTimeInMinutes);
        }

        public static async Task<T> GetAsync<T>(string key, Func<Task<T>> loadCache, double expirationTimeInMinutes)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("key cannot be null, empty, or only whitespace.");
            }

            IDatabase cache = Connection.GetDatabase();
            T value = await GetAsync<T>(cache, key).ConfigureAwait(false);
            if (value == null)
            {
                value = await loadCache().ConfigureAwait(false);
                if (value != null)
                {
                    await SetAsync(cache, key, value, expirationTimeInMinutes).ConfigureAwait(false);
                }
            }

            return value;
        }

        public static async Task FlushAsync()
        {
            // In order to flush all, we need to be in admin mode.
            var options = ConfigurationOptions.Parse(CloudConfigurationManager.GetSetting("RedisConfiguration"));
            options.AllowAdmin = true;

            using (var adminConnection = ConnectionMultiplexer.Connect(options))
            {
                foreach (var redisEndPoint in adminConnection.GetEndPoints(true))
                {
                    IServer server = adminConnection.GetServer(redisEndPoint);
                    await server.FlushAllDatabasesAsync().ConfigureAwait(false);
                }

                await adminConnection.CloseAsync(true).ConfigureAwait(false);
            }
        }

        private static async Task<T> GetAsync<T>(IDatabase cache, string key)
        {
            var json = await cache.StringGetAsync(key).ConfigureAwait(false);
            return Deserialize<T>(json);
        }

        private static async Task SetAsync(IDatabase cache, string key, object value, double expirationTimeInMinutes)
        {
            await cache.StringSetAsync(key, Serialize(value)).ConfigureAwait(false);

            // We will default to a five minute expiration
            await cache.KeyExpireAsync(key, TimeSpan.FromMinutes(expirationTimeInMinutes)).ConfigureAwait(false);
        }

        private static string Serialize(object o)
        {
            if (o == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(o);
        }

        private static T Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
