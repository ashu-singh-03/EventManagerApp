using EventManager.Application.Interfaces;
using EventManager.Infrastructure.Data;
using EventManager.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Register Dapper context
            services.AddSingleton<DapperContext>();

            // Register repositories
            services.AddScoped<IParticipantRepository, ParticipantRepository>();
            services.AddScoped<IEventRepository, EventRepository>(); // <-- Add this
            services.AddScoped<ITicketTypeRepository, TicketTypeRepository>(); // <-- Add this
            services.AddScoped<IAccessPointRepository, AccessPointRepository>();
            services.AddScoped<ITicketAccessPointRepository, TicketAccessPointRepository>();
            services.AddScoped<ITicketParticipantsRepository, TicketParticipantsRepository>();
            return services;
        }
    }
}
