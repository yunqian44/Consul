using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul.WebApi.Ocelot.Common;
using Consul.WebApi.Ocelot.Extension;
using IdentityServer4.AccessTokenValidation;
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
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IWebHostEnvironment Environment { get; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton(new Appsettings(Environment.ContentRootPath));


            //注入Ocelot 配置信息

            //版本一  Ocelot GeteWay
            //services.AddOcelot(new ConfigurationBuilder()
            //        .AddJsonFile("configuration.json")
            //        .Build());

            // specified IdentityServer 4 configuration
            #region IdentityServerAuthenticationOptions => need to refactor
            Action<IdentityServerAuthenticationOptions> isaOptClient = option =>
            {
                option.Authority = Appsettings.app(new string[] { "Idps", "AuthorityUrl" });
                option.ApiName = Appsettings.app(new string[] { "Idps", "ApiNames", "ValuesServiceName" });
                option.RequireHttpsMetadata = Appsettings.app(new string[] { "Idps", "RequireHttps" }).ObjToBool();
                //option.SupportedTokens = SupportedTokens.Both;
                option.ApiSecret = Appsettings.app(new string[] { "Idps", "ApiNames", "ValuesServiceName" });
            };

            Action<IdentityServerAuthenticationOptions> isaOptProduct = option =>
            {
                option.Authority = Appsettings.app(new string[] { "Idps", "AuthorityUrl" });
                option.ApiName = Appsettings.app(new string[] { "Idps", "ApiNames", "ProductServiceName" });
                option.RequireHttpsMetadata = Appsettings.app(new string[] { "Idps", "RequireHttps" }).ObjToBool();
                //option.SupportedTokens = SupportedTokens.Both;
                option.ApiSecret = Appsettings.app(new string[] { "Idps", "ApiNames", "ProductService" });
            };
            #endregion

            services.AddAuthentication()
                .AddIdentityServerAuthentication("ValuesServiceKey", isaOptClient)
                .AddIdentityServerAuthentication("ProductServiceKey", isaOptProduct);


            //版本二  Ocelot GeteWay+Consul
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
