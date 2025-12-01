using EventManager.Application.Interfaces;
using EventManager.Infrastructure.Data;
using EventManager.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddSingleton<DapperContext>();
            return services;
        }
    }
}
