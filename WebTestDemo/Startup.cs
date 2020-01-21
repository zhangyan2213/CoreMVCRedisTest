using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebTestDemo.Helper.Redis;

namespace WebTestDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            #region Redis
            services.AddSingleton<IRedisConnect, RedisConnect>();
            services.AddSingleton<IRedisHelper, RedisHelper>();
            services.Configure<RedisConfigDto>(Configuration.GetSection("Redis"));//从Appsetting中获取设置的值映射到对象中，可以使用IOptions<T>注入到项目中

            //var section = Configuration.GetSection("Redis:RedisDefault");
            //var redisConnection = section.GetSection("Connection")?.Value;
            //var _instanceName = section.GetSection("InstanceName")?.Value; 
            //int _defaultDB = int.Parse(section.GetSection("DefaultDB")?.Value ?? "0"); 
            //var sentinelSection = Configuration.GetSection("Redis:RedisSentinelDefault:Connection")?.Value; //哨兵连接
            //services.AddSingleton(new RedisHelper(redisConnection, _instanceName, _defaultDB, sentinelSection));
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
