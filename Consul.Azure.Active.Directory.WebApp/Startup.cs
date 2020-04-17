using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul.Azure.Active.Directory.WebApp.Common;
using Consul.Azure.Active.Directory.WebApp.Extension;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;

namespace Consul.Azure.Active.Directory.WebApp
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
            services.AddSingleton(new Appsettings(Environment.ContentRootPath));

            services.AddAuthentication(options=>{

                //客户端应用设置使用"Cookies"进行认证
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                // identityserver4设置使用"oidc"进行认证
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;

            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
           // 对使用的OpenIdConnect进行设置，此设置与Identityserver的config.cs中相应client配置一致才可能登录授权成功
           .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
           {
               options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
               options.Authority = Appsettings.app(new string[] { "Azure_AD_B2C", "AuthorityUrl" });
               options.RequireHttpsMetadata = Appsettings.app(new string[] { "Azure_AD_B2C", "RequireHttpsMetadata" }).ObjToBool();//必须https协议
               options.ClientId = Appsettings.app(new string[] { "Azure_AD_B2C", "ClientId" }); ;//Azure AD B2C项目中配置的client
               options.ClientSecret = Appsettings.app(new string[] { "Azure_AD_B2C", "ClientSecret" }); ;
               options.SaveTokens = Appsettings.app(new string[] { "Azure_AD_B2C", "SaveTokens" }).ObjToBool();
               options.ResponseType = Appsettings.app(new string[] { "Azure_AD_B2C", "ResponseType" }); ;//响应类型
               // 下边是所有的scope,必须要和idp项目中一致,至少是一部分
               options.Scope.Clear();
               options.Scope.Add("openid");//"openid"
               options.Scope.Add("offline_access");//"offline_access"
           });

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            IdentityModelEventSource.ShowPII = true; // here

            // open authentication middleware
            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
