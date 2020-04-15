using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul.WebApi.IdentityServer.AuthHelper;
using Consul.WebApi.IdentityServer.Common;
using Consul.WebApi.IdentityServer.ConfigCenter;
using Consul.WebApi.IdentityServer.Helper;
using Consul.WebApi.IdentityServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Consul.WebApi.IdentityServer
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new GetTableData(Environment.ContentRootPath));
            services.AddSingleton(new Appsettings(Environment.ContentRootPath));


            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IRoleService, RoleService>();
            services.AddSingleton<IUserRoleService, UserRoleService>();

            #region ����IdentityServer4
            var builder = services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                })
                // in-memory, code config
                .AddTestUsers(InMemoryConfiguration.Users().ToList())
                .AddInMemoryApiResources(InMemoryConfiguration.GetApiResources())
                .AddInMemoryClients(InMemoryConfiguration.GetClients())
                .AddInMemoryIdentityResources(InMemoryConfiguration.GetIdentityResources());
                //.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
                //.AddProfileService<CustomProfileService>();


            builder.AddDeveloperSigningCredential();

            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();
            }
            else
            {
                throw new Exception("need to configure key material");
            }
            #endregion
            services.AddRazorPages();
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
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            //open ids4 authorization 
            app.UseIdentityServer();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
