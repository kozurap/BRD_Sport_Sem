using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
            services.AddRouting();
            services.AddMvcPattern();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IControllerActivator controllerActivator, IActionActivator actionActivator)
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

            _routeHelper = new RouteHelper(controllerActivator, actionActivator);
            var routeBuilder = new RouteBuilder(app);
            _routeHelper.Initialize(routeBuilder);

            //app.UseHttpsRedirection();
            app.UseAuthentication();   
            app.UseAuthorization(); 
            app.UseStaticFiles();
            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.UseRouter(routeBuilder.Build());
        }
    }
}