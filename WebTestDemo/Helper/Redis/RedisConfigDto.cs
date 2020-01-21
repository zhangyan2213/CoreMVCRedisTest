using System.Collections.Generic;
using System.Linq;

namespace WebTestDemo.Helper.Redis
{
    public class RedisConfigDto
    {
        /// <summary>
        /// Redis连接字符串
        /// </summary>
        public string RedisConnectionStr { get; set; }
        /// <summary>
        /// 实例名称（类似命名空间）
        /// </summary>
        public string RedisInstanceName { get; set; }
        /// <summary>
        /// 默认数据库
        /// </summary>
        public int RedisDefaultDB { get; set; }
        /// <summary>
        /// Redis哨兵连接字符串
        /// </summary>
        public string SentinelConnectionStr { get; set; }
        public List<string> RedisConnectionList { get; set; }
        public List<string> SentinelConnectionList { get; set; }
    }
}
