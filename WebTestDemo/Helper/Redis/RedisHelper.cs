using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace WebTestDemo.Helper.Redis
{
    public class RedisHelper : IRedisHelper
    {
        public static IRedisConnect _redisConnect;
        public static IDatabase _redis;
        private static Timer _timer;
        private static int _inTimer = 0;
        
        public RedisHelper(IOptions<RedisConfigDto> redisConfigOptions,IRedisConnect redisConnect)
        {
            _redisConnect = redisConnect;
            _redis = _redisConnect.GetDatabase();
        }

        #region 获取redis信息
        /// <summary>
        /// 获取数据库
        /// </summary>
        /// <returns></returns>
        public IDatabase GetDatabase(int defaultDB = -1)
        {
            return _redis;
        }
        public IServer GetServer(string configName = null, int endPointsIndex = 0)
        {
            return _redisConnect.GetServer(configName, endPointsIndex);
        }
        public ISubscriber GetSubscriber(string configName = null)
        {
            return _redisConnect.GetSubscriber(configName);
        }
        #endregion

        #region 实现分布式锁
        /// <summary>
        /// 获取锁
        /// </summary>
        /// <param name="key">要操作的Key</param>
        /// <param name="value">Guid</param>
        /// <param name="second">过期时间(秒)</param>
        /// <returns></returns>
        public async Task<bool> LockTakeAsync(string key, string value, int second)
        {
            return await _redis.LockTakeAsync(key, value, TimeSpan.FromSeconds(second));
        }
        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> LockReleaseAsync(string key, string value)
        {
            return await _redis.LockReleaseAsync(key, value);
        }
        /// <summary>
        /// Redis分布式锁续期问题解决方案
        /// 设置redis分布式锁情况下，客户端获取锁成功执行redis操作时，同时开启一个后台线程（看门狗），
        /// 每隔过期时间的1/3时检查是否还持有锁，如果持有则自动续期
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="second"></param>
        public void LockWatchDogStart(string key, string value, int second)
        {
            _timer = new Timer(second * 1000 / 3.0);
            _timer.Elapsed += (obj, evt) =>
            {
                LockRenew(key, value, second);
            };
            _timer.Start();
        }
        private static void LockRenew(string key, string value, int second)
        {
            if (System.Threading.Interlocked.Exchange(ref _inTimer,1) == 0)
            {
                _redis.KeyExpire(key, DateTime.Now.AddSeconds(second));
                //var redisValue = _redis.StringGet(key);
                //if (redisValue == value)
                //{
                //    Console.WriteLine($"--设置前剩余过期时间为{_redis.KeyTimeToLive(key)}");
                //    _redis.KeyExpire(key, DateTime.Now.AddSeconds(second));
                //    Console.WriteLine($"***设置后剩余过期时间为{_redis.KeyTimeToLive(key)}");
                //}
            }
        }
        public void LockWatchDogStop()
        {
            //Console.WriteLine($"Stop——{System.Threading.Thread.CurrentThread.ManagedThreadId}关闭开门狗，时间为:{DateTime.Now}");
            System.Threading.Interlocked.Exchange(ref _inTimer, 0);
            _timer.Stop();
        }
        #endregion

    }
}
