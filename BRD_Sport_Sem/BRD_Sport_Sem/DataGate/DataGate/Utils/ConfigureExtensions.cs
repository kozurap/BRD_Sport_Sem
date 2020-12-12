using System.Linq;
using System.Net;
using DataGate.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace DataGate.Utils
{
    public static class ConfigureExtensions
    {
        public static IServiceCollection AddDataGateServices(this IServiceCollection services)
        {
            services.AddScoped<TransactionService>();
            services.AddScoped<DataGateORM>();
            services.AddScoped<DataContextExecutor>();
            services.AddScoped<DataContextReceiver>();

            return services;
        }
    }
}