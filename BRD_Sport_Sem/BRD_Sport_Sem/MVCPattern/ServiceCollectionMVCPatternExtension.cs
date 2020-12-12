﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using ProjectArt.MVCPattern.ActionResults;
using ProjectArt.MVCPattern.Services;

namespace ProjectArt.MVCPattern
{
    public static class ServiceCollectionMvcPatternExtensions
    {
        public static IServiceCollection AddMvcPattern(this IServiceCollection services)
        {
            services.AddMvcCore().AddRazorViewEngine();
            var physicalFileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
            services.AddSingleton(physicalFileProvider);
            services.AddScoped<IViewRenderService, ViewRenderService>();
            services.AddTransient<IControllerActivator, DefaultControllerActivator>();
            services.AddTransient<IActionActivator, DefaultActionActivator>();
            foreach (var actionResultClass in Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.GetInterfaces().Contains(typeof(IActionResult))))
            {
                var actionProviderServiceType = typeof(IActionResultProvider<>).MakeGenericType(new[] {actionResultClass});
                var actionProviderType = typeof(ActionResultProvider<>).MakeGenericType(new[] {actionResultClass});
                services.AddScoped(actionProviderServiceType, actionProviderType);
            }
            
            foreach (var valueProviderSource in Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.IsSubclassOf(typeof(ValueProviderSource))))
                services.AddScoped(valueProviderSource);
            return services;
        }
    }
}