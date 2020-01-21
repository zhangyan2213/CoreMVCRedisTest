using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebTestDemo.Helper.Redis
{
    public interface IRedisConnect
    {
        IDatabase GetDatabase(int defaultDB = -1);
        IServer GetServer(string configName = null, int endPointsIndex = 0);
        ISubscriber GetSubscriber(string configName = null);
    }
}
