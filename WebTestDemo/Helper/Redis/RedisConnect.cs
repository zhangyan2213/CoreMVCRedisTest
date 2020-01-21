using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebTestDemo.Helper.Redis
{
    public class RedisConnect: IRedisConnect,IDisposable
    {
        private static ConcurrentDictionary<string, ConnectionMultiplexer> _connections;
        private ConfigurationOptions _configurationOptions;
        private readonly string _instanceName; //实例名称
        private readonly int _defaultDB = 0; //默认数据库
        private static ConnectionMultiplexer _sentinel;
        private static ConfigurationOptions _sentinelConfigurationOptions;
        private static ISubscriber _sentinelsub;
        public static IDatabase _redis;
        public static RedisConfigDto _redisConfig { get; set; }
        public RedisConnect(IOptions<RedisConfigDto> redisConfigOptions)
        {
            _redisConfig = redisConfigOptions.Value;
            _instanceName = _redisConfig.RedisInstanceName;
            _defaultDB = _redisConfig.RedisDefaultDB;
            RedisConfig();
        }
        /// <summary>
        /// 配置Redis
        /// </summary>
        private void RedisConfig()
        {
            if (_redisConfig.RedisConnectionList.Count == 0)
            {
                throw new Exception("Redis配置有误！");
            }
            _connections = new ConcurrentDictionary<string, ConnectionMultiplexer>();
            _configurationOptions = new ConfigurationOptions();
            _redisConfig.RedisConnectionList.ForEach(r => _configurationOptions.EndPoints.Add(r));
            if (_redisConfig.SentinelConnectionList.Count > 0)
            {
                SentinelConfig();
                SubSentinel();
            }
        }

        #region 配置哨兵
        /// <summary>
        /// 配置哨兵
        /// </summary>
        public void SentinelConfig()
        {
            _sentinelConfigurationOptions = new ConfigurationOptions
            {
                TieBreaker = "", //哨兵模式一定要写
                CommandMap = CommandMap.Sentinel,
                ServiceName = "mymaster"
            };
            _redisConfig.SentinelConnectionList.ForEach(s => _sentinelConfigurationOptions.EndPoints.Add(s));
            _sentinel = ConnectionMultiplexer.Connect(_sentinelConfigurationOptions);
            _sentinelsub = _sentinel.GetSubscriber();
        }
        /// <summary>
        /// 订阅哨兵，主从redis切换时系统随之切换
        /// </summary>
        public void SubSentinel()
        {
            _sentinelsub.Subscribe("+switch-master", (channel, message) =>
            {
                //Todo 记录日志
            });
        }
        #endregion

        #region 获取redis信息
        /// <summary>
        /// 获取ConnectionMultiplexer
        /// </summary>
        /// <returns></returns>
        private ConnectionMultiplexer GetConnect()
        {
            return _connections.GetOrAdd(_instanceName, p => ConnectionMultiplexer.Connect(_configurationOptions));
        }
        /// <summary>
        /// 获取数据库
        /// </summary>
        /// <returns></returns>
        public IDatabase GetDatabase(int defaultDB = -1)
        {
            if (defaultDB < 0)
            {
                defaultDB = _defaultDB;
            }
            _redis = GetConnect().GetDatabase(defaultDB);
            return _redis;
        }
        public IServer GetServer(string configName = null, int endPointsIndex = 0)
        {
            return GetConnect().GetServer(_configurationOptions.EndPoints[endPointsIndex]);
        }
        public ISubscriber GetSubscriber(string configName = null)
        {
            return GetConnect().GetSubscriber();
        }
        #endregion
        public void Dispose()
        {
            if (_connections != null && _connections.Count > 0)
            {
                foreach (var item in _connections.Values)
                {
                    item.Close();
                }
            }
        }
    }
}
