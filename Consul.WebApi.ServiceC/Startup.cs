using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Consul.WebApi.ServiceC.Common;
using Consul.WebApi.ServiceC.Extension;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Consul.WebApi.ServiceC
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddSingleton(new Appsettings(Environment.ContentRootPath));

            // IdentityServer
            services.AddAuthentication("Bearer")
                 .AddIdentityServerAuthentication(options =>
                 {
                     options.Authority = Appsettings.app(new string[] { "Idps", "AuthorityUrl" });
                     options.RequireHttpsMetadata = Appsettings.app(new string[] { "Idps", "RequireHttps" }).ObjToBool();
                     options.ApiName = Appsettings.app(new string[] { "Idps", "ApiName" });
                 });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            // open authentication middleware
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
