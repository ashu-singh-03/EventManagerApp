using EventManager.Application.Interfaces;
using EventManager.Application.Services;
using EventManager.Application.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace EventManager.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IParticipantService, ParticipantService>();
            services.AddScoped<ITicketTypeService, TicketTypeService>();
            services.AddScoped<IAccessPointService, AccessPointService>();
            services.AddScoped<ITicketAccessPointService, TicketAccessPointService>();
            services.AddScoped<IEventClaimService, EventClaimService>();
            services.AddScoped<ITicketParticipantsService, TicketParticipantsService>();
            services.AddScoped<IParticipantCommunicationService, ParticipantCommunicationService>();
            services.AddScoped<IScanService, ScanService>();
            services.AddHttpClient<IMailgunService, MailgunService>();

            services.AddScoped<ExcelHelper>();

            return services;
        }
    }
}