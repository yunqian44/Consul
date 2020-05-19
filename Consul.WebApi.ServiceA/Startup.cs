using System;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Autofac;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Consul.WebApi.Consul;
using Consul.WebApi.Extensions;
using Consul.WebApi.ServiceA.AOP;
using Consul.WebApi.ServiceA.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Consul.WebApi
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

            //缓存注入
            services.AddSingleton<IMemoryCache>(factory =>
            {
                var cache = new MemoryCache(new MemoryCacheOptions());
                return cache;
            });


            #region CORS
            //跨域第二种方法，声明策略，记得下边app中配置
            services.AddCors(c =>
            {
                //↓↓↓↓↓↓↓注意正式环境不要使用这种全开放的处理↓↓↓↓↓↓↓↓↓↓
                //c.AddPolicy("AllRequests", policy =>
                //{
                //    policy
                //    .AllowAnyOrigin()//允许任何源
                //    .AllowAnyMethod()//允许任何方式
                //    .AllowAnyHeader()//允许任何头
                //    .AllowCredentials();//允许cookie
                //});
                //↑↑↑↑↑↑↑注意正式环境不要使用这种全开放的处理↑↑↑↑↑↑↑↑↑↑


                //一般采用这种方法
                c.AddPolicy("LimitRequests", policy =>
                {
                    policy
                    .WithOrigins("http://localhost:8080")//支持多个域名端口，注意端口号后不要带/斜杆：比如localhost:8000/，是错的
                    .AllowAnyHeader()//Ensures that the policy allows any header.
                    .AllowAnyMethod();
                });
            });
            #endregion


            services.AddControllers();


            //services.AddSingleton<HystrixAOP>();
            services.AddScoped<ProductService>();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            #region AutoFac DI
            //注册要通过反射创建的组件
            //builder.RegisterType<HystrixAOP>();//可以直接替换其他拦截器

            //builder.RegisterType<ProductService>()
            //   .EnableClassInterceptors()
            //   .InterceptedBy(typeof(HystrixAOP));
            #endregion

            foreach (Type type in typeof(Program).Assembly.GetExportedTypes())
            {
                //判断类中是否有标注了 CustomInterceptorAttribute 的方法
                bool hasCustomInterceptorAttr = type.GetMethods()
                 .Any(m => m.GetCustomAttribute(typeof(HystrixCommandAttribute)) != null);
                if (hasCustomInterceptorAttr)
                {
                    builder.RegisterAssemblyTypes(type.Assembly).AsImplementedInterfaces();
                }
            }
            builder.RegisterDynamicProxy();

            //builder.RegisterAssemblyTypes(t).
            //Where(x => x.Name.EndsWith("service", StringComparison.OrdinalIgnoreCase)).AsImplementedInterfaces();
            //builder.RegisterDynamicProxy();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            #region CORS(跨越资源共享)
            //跨域第一种版本，启用跨域策略  不推荐使用
            //app.UseCors("AllowAllOrigin"); 

            //跨域第二种版本，请要ConfigureService中配置服务 services.AddCors();
            //    app.UseCors(options => options.WithOrigins("http://localhost:8021").AllowAnyHeader()
            //.AllowAnyMethod()); 

            //跨域第三种方法，使用策略，详细策略信息在ConfigureService中
            app.UseCors("LimitRequests");//将 CORS 中间件添加到 web 应用程序管线中, 以允许跨域请求。
            #endregion

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //var consulOption = new ConsulOption
            //{
            //    ServiceName = Configuration["ServiceName"],
            //    ServiceIP = Configuration["ServiceIP"],
            //    ServicePort = Convert.ToInt32(Configuration["ServicePort"]),
            //    ServiceHealthCheck = Configuration["ServiceHealthCheck"],
            //    Address = Configuration["ConsulAddress"]
            //};
            //app.RegisterConsul(lifetime, consulOption);


            // 短路中间件，配置Controller路由
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
