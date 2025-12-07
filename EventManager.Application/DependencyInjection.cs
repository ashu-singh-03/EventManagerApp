using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http; // For IHttpContextAccessor

namespace EventManager.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Existing services
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IParticipantService, ParticipantService>();
            services.AddScoped<ITicketTypeService, TicketTypeService>();
            services.AddScoped<IAccessPointService, AccessPointService>();
            // Add EventClaimService
            services.AddScoped<IEventClaimService, EventClaimService>();

            return services;
        }
    }
}
