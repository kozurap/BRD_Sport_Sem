using BRD_Sport_Sem.Models;
using DataGate.Core;
using DataGate.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjectArt.MVCPattern;
using ProjectArt.MVCPattern.Services;

namespace BRD_Sport_Sem
{
    public class Startup
    {
        private RouteHelper _routeHelper;
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();
            services.AddRouting();
            services.AddDataGateServices();
            services.AddMvcPattern();
            services.AddSession();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IControllerActivator controllerActivator, IActionActivator actionActivator,IConfiguration configuration)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                //app.UseHsts();
            }
            
            DataGateORM.Connect(configuration.GetConnectionString("postgres"));
            TableRegistry();
            
            app.UseSession();

            _routeHelper = new RouteHelper(controllerActivator, actionActivator);
            var routeBuilder = new RouteBuilder(app);
            _routeHelper.Initialize(routeBuilder);
            app.UseStaticFiles();
            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.UseRouter(routeBuilder.Build());
        }

        private void TableRegistry()
        {
            DataGateORM.Register<User>("users");
            DataGateORM.Register<Forum>("forum");
            DataGateORM.Register<Tournament>("tournaments");
            DataGateORM.Register<Record>("records");
        }
    }
}