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
        private readonly int lockKeyOutTime = 30;

        public SnapUpTestController(IRedisHelper redisClient)
        {
            _redisClient = redisClient;
            _redis = redisClient.GetDatabase();
        }

        //模拟抢购
        public void SnapUpTest()
        {
            Parallel.For(0, 1000, thread =>
            {
                for (int i = 0; i < 20; i++)
                {
                    SnapUpCommodity();
                    Thread.Sleep(50);
                }
            });
            Console.WriteLine($"抢购结束——————:{DateTime.Now}");
        }

        public async Task SnapUpCommodity()
        {
            string guid = Guid.NewGuid().ToString();
            var threadId = Thread.CurrentThread.ManagedThreadId.ToString();
            var commoditySum = int.Parse(await _redis.StringGetAsync(commodityKey));
            if (commoditySum <= 0) 
            {
                return;
            }
            var rushBuySuccess = _redis.HashExists(RushBuySuccessUser,threadId);
            if (rushBuySuccess == true)
            {
                return;
            }
            var lockResult = await _redisClient.LockTakeAsync(lockKey, guid, lockKeyOutTime);
            if (lockResult)
            {
                try
                {
                    await SanpUpCommodity();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    _redis.HashIncrement(RushBuySuccessUser, threadId);
                    await _redisClient.LockReleaseAsync(lockKey, guid);
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
