using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Existing service
            services.AddScoped<IEventService, EventService>();

            // Add Participant service
            services.AddScoped<IParticipantService, ParticipantService>();
            services.AddScoped<ITicketTypeService, TicketTypeService>();

            return services;
        }
    }
}
