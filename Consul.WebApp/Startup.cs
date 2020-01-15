using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Consul.WebApp.Common;
using Consul.WebApp.Extension;
using IdentityModel;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Consul.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration,IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IWebHostEnvironment Environment { get; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddSingleton(new Appsettings(Environment.ContentRootPath));

            //关闭默认映射，否则它可能修改从授权服务返回的各种claim属性
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            // IdentityServer open id and connect
            #region ids 4 configuration  方式2
            services.AddAuthentication(options =>
                {
                    //客户端应用设置使用"Cookies"进行认证
                    //options.DefaultScheme = "Cookies";
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                    // oidc => open ID connect 设置使用"oidc"进行认证
                    //options.DefaultChallengeScheme = "oidc"; 
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.Authority = Appsettings.app(new string[] { "Idps", "AuthorityUrl" }); ;

                    // please use https in production env
                    options.RequireHttpsMetadata = Appsettings.app(new string[] { "Idps", "RequireHttps" }).ObjToBool();
                    options.ClientId = Appsettings.app(new string[] { "Idps", "ClientId" });
                    options.ResponseType = Appsettings.app(new string[] { "Idps", "ResponseType" }); // allow to return access token
                    options.ClientSecret = "secret";
                    options.SaveTokens = Appsettings.app(new string[] { "Idps", "SaveTokens" }).ObjToBool();

                    
                    //下面所有的scope 必须和idp项目中一致，至少是一部分

                    options.Scope.Clear();
                    
                    options.Scope.Add(OidcConstants.StandardScopes.OpenId);
                    options.Scope.Add(OidcConstants.StandardScopes.Profile);
                    options.Scope.Add(OidcConstants.StandardScopes.Email);
                    options.Scope.Add("roles");
                    options.Scope.Add("rolename");
                });
            #endregion

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

            app.UseRouting();

            // open authentication middleware
            app.UseAuthentication();

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
