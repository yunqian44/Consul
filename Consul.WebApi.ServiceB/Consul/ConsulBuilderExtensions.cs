using Consul.WebApi.ServiceB.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Consul.WebApi.ServiceB.Consul
{
    public static class ConsulBuilderExtensions
    {
        public static IApplicationBuilder RegisterConsul(this IApplicationBuilder app, IHostApplicationLifetime lifetime, ConsulOption consulOption)
        {
            var ConsulAddress0 = Appsettings.app("ConsulAddress","0");
            var consulClient = new ConsulClient(x =>
            {
                // consul 服务地址
                x.Address = new Uri(ConsulAddress0);
            });

            var registration = new AgentServiceRegistration()
            {
                ID = Guid.NewGuid().ToString(),
                Name = consulOption.ServiceName,// 服务名
                Address = consulOption.ServiceIP, // 服务绑定IP
                Port = consulOption.ServicePort, // 服务绑定端口
                Check = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//服务启动多久后注册
                    Interval = TimeSpan.FromSeconds(10),//健康检查时间间隔
                    HTTP = consulOption.ServiceHealthCheck,//健康检查地址
                    Timeout = TimeSpan.FromSeconds(5)
                }
            };

            // 服务注册
            consulClient.Agent.ServiceRegister(registration).Wait();

            // 应用程序终止时，服务取消注册
            lifetime.ApplicationStopping.Register(() =>
            {
                consulClient.Agent.ServiceDeregister(registration.ID).Wait();
            });


            var ConsulAddress1 = Appsettings.app("ConsulAddress", "1");
            var consulClient1 = new ConsulClient(x =>
            {
                // consul 服务地址
                x.Address = new Uri(ConsulAddress1);
            });

            var registration1 = new AgentServiceRegistration()
            {
                ID = Guid.NewGuid().ToString(),
                Name = consulOption.ServiceName,// 服务名
                Address = consulOption.ServiceIP, // 服务绑定IP
                Port = consulOption.ServicePort, // 服务绑定端口
                Check = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),//服务启动多久后注册
                    Interval = TimeSpan.FromSeconds(10),//健康检查时间间隔
                    HTTP = consulOption.ServiceHealthCheck,//健康检查地址
                    Timeout = TimeSpan.FromSeconds(5)
                }
            };

            // 服务注册
            consulClient1.Agent.ServiceRegister(registration1).Wait();

            // 应用程序终止时，服务取消注册
            lifetime.ApplicationStopping.Register(() =>
            {
                consulClient1.Agent.ServiceDeregister(registration1.ID).Wait();
            });

            return app;
        }
    }
}
