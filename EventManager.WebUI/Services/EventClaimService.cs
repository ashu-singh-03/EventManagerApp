using EventManager.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EventManager.Application.Services
{
    public class EventClaimService : IEventClaimService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EventClaimService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ??
                throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task SetEventIdClaimAsync(int eventId)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                throw new InvalidOperationException("HttpContext is null.");

            var claims = new List<Claim>
            {
                new Claim("EventId", eventId.ToString())
            };

            var identity = new ClaimsIdentity(claims, "EventCookie");
            var principal = new ClaimsPrincipal(identity);

            // Sign in with the EventCookie scheme
            await httpContext.SignInAsync("EventCookie", principal);
        }
        public int GetEventIdFromClaim()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User == null)
                return 0;

            var claim = httpContext.User.FindFirst("EventId");
            return claim != null && int.TryParse(claim.Value, out int id) ? id : 0;
        }
    }
}
