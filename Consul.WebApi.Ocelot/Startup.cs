using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Polly;

namespace Consul.WebApi.Ocelot
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
            services.AddControllers();

            //注入Ocelot 配置信息

            //版本一  Ocelot GeteWay
            //services.AddOcelot(new ConfigurationBuilder()
            //        .AddJsonFile("configuration.json")
            //        .Build());

            //版本一  Ocelot GeteWay+Consul
            services.AddOcelot(new ConfigurationBuilder()
                    .AddJsonFile("configuration.json")
                    .Build()).AddConsul().AddPolly();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            //启用Ocelot 网关中间件
            app.UseOcelot().Wait();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
