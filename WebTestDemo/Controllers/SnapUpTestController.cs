using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebTestDemo.Helper.Redis;

namespace WebTestDemo.Controllers
{
    public class SnapUpTestController : Controller
    {
        private readonly IDatabase _redis;
        private readonly IRedisHelper _redisClient;
        private readonly static string commodityKey = "CommodityKey";
        private readonly string lockKey = $"Lock:{commodityKey}";
        private readonly string RushBuySuccessUser = "RushBuySuccess";
        private readonly int lockKeyOutTime = 15;

        public SnapUpTestController(IRedisHelper redisClient)
        {
            _redisClient = redisClient;
            _redis = redisClient.GetDatabase();
        }

        //模拟抢购
        public void SnapUpTest()
        {
            Console.WriteLine($"抢购开始——————:{DateTime.Now.ToLongTimeString()}");
            Parallel.For(0, 1000, thread => //模拟N个人，多线程异步
            {
                for (int i = 0; i < 10; i++) //模拟一个人点击N次，每次间隔50毫秒
                {
                    SnapUpCommodity();
                    Thread.Sleep(50);
                }
            });
            Console.WriteLine($"抢购结束——————:{DateTime.Now.ToLongTimeString()}");
        }

        public async Task SnapUpCommodity()
        {
            string guid = Guid.NewGuid().ToString();//设置唯一key，互斥锁，只能解锁自己上的锁
            var threadId = Thread.CurrentThread.ManagedThreadId.ToString(); //线程id 模拟UserID
            var commoditySum = int.Parse(await _redis.StringGetAsync(commodityKey, CommandFlags.PreferSlave)); //模拟库存库存 如果库存不足，则直接返回
            if (commoditySum <= 0 || _redis.HashExists(RushBuySuccessUser, threadId, CommandFlags.PreferSlave))   //根据业务，防止超抢情况，如果抢购成功则直接返回
            {
                return;
            }
            var lockResult = await _redisClient.LockTakeAsync(lockKey, guid, lockKeyOutTime);//获取锁，设置定期时间，一般为30秒，防止执行过程中服务器宕机导致死锁
            if (lockResult) //如果获取成功则执行业务
            {
                try
                {
                    //_redisClient.LockWatchDogStart(lockKey, guid, lockKeyOutTime);
                    Thread.Sleep(4000);
                    await SanpUpCommodity();//模拟抢购成功，减库存
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    //_redisClient.LockWatchDogStop();
                    _redis.HashIncrement(RushBuySuccessUser, threadId);//如果抢购成功则存入hashkey，避免超抢
                    await _redisClient.LockReleaseAsync(lockKey, guid);//抢购成功则释放锁
                    Console.WriteLine($"用户：{threadId}已抢到商品，时间为:{DateTime.Now}");
                }   
            }
        }

        private async Task<long> SanpUpCommodity()
        {
            return await _redis.StringDecrementAsync(commodityKey);
        }
    }
}
