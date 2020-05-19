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

            //����ע��
            services.AddSingleton<IMemoryCache>(factory =>
            {
                var cache = new MemoryCache(new MemoryCacheOptions());
                return cache;
            });


            #region CORS
            //����ڶ��ַ������������ԣ��ǵ��±�app������
            services.AddCors(c =>
            {
                //��������������ע����ʽ������Ҫʹ������ȫ���ŵĴ����������������������
                //c.AddPolicy("AllRequests", policy =>
                //{
                //    policy
                //    .AllowAnyOrigin()//�����κ�Դ
                //    .AllowAnyMethod()//�����κη�ʽ
                //    .AllowAnyHeader()//�����κ�ͷ
                //    .AllowCredentials();//����cookie
                //});
                //��������������ע����ʽ������Ҫʹ������ȫ���ŵĴ����������������������


                //һ��������ַ���
                c.AddPolicy("LimitRequests", policy =>
                {
                    policy
                    .WithOrigins("http://localhost:8080")//֧�ֶ�������˿ڣ�ע��˿ںź�Ҫ��/б�ˣ�����localhost:8000/���Ǵ��
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
            //ע��Ҫͨ�����䴴�������
            //builder.RegisterType<HystrixAOP>();//����ֱ���滻����������

            //builder.RegisterType<ProductService>()
            //   .EnableClassInterceptors()
            //   .InterceptedBy(typeof(HystrixAOP));
            #endregion

            foreach (Type type in typeof(Program).Assembly.GetExportedTypes())
            {
                //�ж������Ƿ��б�ע�� CustomInterceptorAttribute �ķ���
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

            #region CORS(��Խ��Դ����)
            //�����һ�ְ汾�����ÿ������  ���Ƽ�ʹ��
            //app.UseCors("AllowAllOrigin"); 

            //����ڶ��ְ汾����ҪConfigureService�����÷��� services.AddCors();
            //    app.UseCors(options => options.WithOrigins("http://localhost:8021").AllowAnyHeader()
            //.AllowAnyMethod()); 

            //��������ַ�����ʹ�ò��ԣ���ϸ������Ϣ��ConfigureService��
            app.UseCors("LimitRequests");//�� CORS �м����ӵ� web Ӧ�ó��������, �������������
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


            // ��·�м��������Controller·��
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
