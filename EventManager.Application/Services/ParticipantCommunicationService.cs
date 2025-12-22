using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using QRCoder;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;

namespace EventManager.Application.Services
{
    public class ParticipantCommunicationService : IParticipantCommunicationService
    {
        private readonly IParticipantCommunicationRepository _repository;
        //private readonly IEmailService _emailService;
        private readonly IMailgunService _mailgunService;
        private readonly IConfiguration _configuration;

        public ParticipantCommunicationService(
            IParticipantCommunicationRepository repository,
            //IEmailService emailService,
            IMailgunService mailgunService,
        IConfiguration configuration)
        {
            _repository = repository;
            //_emailService = emailService;
            _mailgunService = mailgunService;
            _configuration = configuration;
        }

        public async Task<List<ParticipantCommunicationDto>> GetParticipantsWithAssignmentsAsync(int eventId)
        {
            return await _repository.GetParticipantsWithAssignmentsAsync(eventId);
        }

        public async Task<EmailResponse> SendEmailToParticipantAsync(int eventId, int participantId)
        {
            try
            {
                // 1. Get email configuration
                var emailConfig = await _repository.GetEmailConfigurationAsync(eventId);
                if (emailConfig == null)
                    return new EmailResponse { Success = false, Error = "No email template configured for this event" };

                // 2. Get participant data
                var participantData = await _repository.GetParticipantEmailDataAsync(eventId, participantId);

                if (participantData == null)
                    return new EmailResponse { Success = false, Error = "Participant not found" };

                // 3. Generate QR Code
                var qrCodeBase64 = GenerateQRCode(participantData.ParticipantCode, eventId);

                // 4. Convert dynamic to proper types
                string subject = emailConfig.Subject?.ToString() ?? "";
                string bodyText = emailConfig.BodyText?.ToString() ?? "";
                string fromEmail = emailConfig.FromEmail?.ToString() ?? "";
                string ccEmail = emailConfig.CcEmail?.ToString() ?? "";
                string bccEmail = emailConfig.BccEmail?.ToString() ?? "";

                // 5. Convert participant data
                string participantCode = participantData.ParticipantCode?.ToString() ?? "";
                string fullName = participantData.FullName?.ToString() ?? "";
                string email = participantData.Email?.ToString() ?? "";
                string company = participantData.Company?.ToString() ?? "";
                string eventName = participantData.EventName?.ToString() ?? "";
                string eventDate = participantData.EventDate?.ToString() ?? "";
                string eventTime = participantData.EventTime?.ToString() ?? "";
                string location = participantData.Location?.ToString() ?? "";
                string ticketType = participantData.TicketTypes?.ToString() ?? ""; 

                // 6. Replace placeholders in email template
                var subjectProcessed = ReplacePlaceholders(subject, eventName, eventDate, eventTime,
                                                          location, fullName, participantCode, company, qrCodeBase64, ticketType);
                var bodyProcessed = ReplacePlaceholders(bodyText, eventName, eventDate, eventTime,
                                                       location, fullName, participantCode, company, qrCodeBase64, ticketType);

                // 7. Create EmailRequest using your existing DTO
                var emailRequest = new EmailRequest
                {
                    FromEmail = fromEmail,
                    FromName = fromEmail?.Split('@')[0] ?? "Event Manager",
                    ToEmails = new List<string> { email },
                    Subject = subjectProcessed,
                    Message = bodyProcessed,
                    IsHtml = true,
                    Tag = $"participant_{participantId}"
                };

                // Add CC emails if any (FIXED - no lambda on dynamic)
                if (!string.IsNullOrEmpty(ccEmail))
                {
                    var ccEmailsList = new List<string>();
                    var ccArray = ccEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var emailAddr in ccArray)
                    {
                        ccEmailsList.Add(emailAddr.Trim());
                    }
                    emailRequest.CcEmails = ccEmailsList;
                }

                // Add BCC emails if any (FIXED - no lambda on dynamic)
                if (!string.IsNullOrEmpty(bccEmail))
                {
                    var bccEmailsList = new List<string>();
                    var bccArray = bccEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var emailAddr in bccArray)
                    {
                        bccEmailsList.Add(emailAddr.Trim());
                    }
                    emailRequest.BccEmails = bccEmailsList;
                }

                emailRequest.FromEmail = "postmaster@sandbox9436a5ace7524a81a718b2e3dd399978.mailgun.org";
                emailRequest.ToEmails[0] = "bviraj44@gmail.com";
                // 8. Send email using your existing EmailService
                return await _mailgunService.SendEmailAsync(emailRequest);
            }
            catch (Exception ex)
            {
                return new EmailResponse
                {
                    Success = false,
                    Error = $"Failed to send email: {ex.Message}"
                };
            }
        }

        private string ReplacePlaceholders(string template, string eventName, string eventDate,
                                         string eventTime, string location, string fullName,
                                         string participantCode, string company, string qrCodeBase64,string ticketType="")
        {
            var result = new StringBuilder(template);

            result.Replace("@@EventName@@", eventName ?? "")
                  .Replace("@@EventDate@@", eventDate ?? "")
                  .Replace("@@EventTime@@", eventTime ?? "")
                  .Replace("@@EventVenue@@", location ?? "")
                  .Replace("@@Location@@", location ?? "")
                  .Replace("@@ParticipantName@@", fullName ?? "")
                  .Replace("@@ParticipantCode@@", participantCode ?? "")
                  .Replace("@@Company@@", company ?? "")
                  .Replace("@@QRCode@@", qrCodeBase64 ?? "")
                  .Replace("@@TicketType@@", ticketType ?? "");

            return result.ToString();
        }

        private string GenerateQRCode(string participantCode, int eventId)
        {
            try
            {
                var qrData = $"EVENT:{eventId}|CODE:{participantCode}|TIME:{DateTime.Now:yyyyMMddHHmmss}";
                using var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new Base64QRCode(qrCodeData);
                var qrCodeImageBase64 = qrCode.GetGraphic(20);
                return $"data:image/png;base64,{qrCodeImageBase64}";
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}