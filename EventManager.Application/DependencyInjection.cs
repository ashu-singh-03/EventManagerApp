using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();
            return services;
        }
    }
}
