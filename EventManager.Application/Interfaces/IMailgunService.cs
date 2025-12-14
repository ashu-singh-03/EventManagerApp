using EventManager.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManager.Application.Interfaces
{
    public interface IMailgunService
    {

        Task<EmailResponse> SendEmailAsync(EmailRequest emailRequest, List<EmailAttachment> attachments = null);
        Task<bool> ValidateCredentialsAsync();
        Task<List<EmailEvent>> GetEmailEventsAsync(string messageId);

    }
}
