using StackExchange.Redis;
using System.Threading.Tasks;

namespace WebTestDemo.Helper.Redis
{
    public interface IRedisHelper
    {
        IDatabase GetDatabase(int defaultDB = -1);
        IServer GetServer(string configName = null, int endPointsIndex = 0);
        ISubscriber GetSubscriber(string configName = null);
        Task<bool> LockTakeAsync(string key, string value, int second);
        Task<bool> LockReleaseAsync(string key, string value);
        /// <summary>
        /// Redis分布式锁续期问题解决方案
        /// 设置redis分布式锁情况下，客户端获取锁成功执行redis操作时，同时开启一个后台线程（看门狗），
        /// 每隔过期时间的1/3时检查是否还持有锁，如果持有则自动续期
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="second"></param>
        void LockWatchDogStart(string key, string value, int second);
        /// <summary>
        /// 设置redis分布式锁情况下，客户端获取锁并执行结束redis操作时，释放后台线程（开门狗）
        /// </summary>
        void LockWatchDogStop();
    }
}
