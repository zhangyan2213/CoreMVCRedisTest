using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ;
using StackExchange.Redis;
using WebTestDemo.Helper.Redis;
using WebTestDemo.Models;

namespace WebTestDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDatabase _redis;
        private readonly IRedisHelper _redisClient;
        public HomeController(IRedisHelper redisClient)
        {
            _redisClient = redisClient;
            _redis = redisClient.GetDatabase();
        }
        public void SendMq(string message)
        {
            RabbitHelp rabbitHelp = new RabbitHelp();
            rabbitHelp.Test(message);
        }
        [HttpPost]
        public string SendRedis([FromBody]JObject jObject)
        {
            if (!jObject.HasValues)
            {
                return "数据错误！";
            }
            string key = jObject.Value<string>("key") ?? "Test1";
            string value = jObject.Value<string>("value") ?? "Value1";
            _redis.StringSet(key, value);
            return _redis.StringGet(key,CommandFlags.PreferSlave);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
