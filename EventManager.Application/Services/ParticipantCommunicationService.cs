using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using QRCoder;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using static EventManager.Application.DTOs.ScanDtos;

namespace EventManager.Application.Services
{
    public class ParticipantCommunicationService : IParticipantCommunicationService
    {
        private readonly IParticipantCommunicationRepository _repository;
        //private readonly IEmailService _emailService;
        private readonly IMailgunService _mailgunService;
        private readonly IConfiguration _configuration;
        private readonly IScanRepository _scanRepository;

        public ParticipantCommunicationService(
            IParticipantCommunicationRepository repository,
             IScanRepository scanRepository,
            //IEmailService emailService,
            IMailgunService mailgunService,
        IConfiguration configuration)
        {
            _repository = repository;
            _scanRepository = scanRepository;  // Assign to field
            //_emailService = emailService;
            _mailgunService = mailgunService;
            _configuration = configuration;
        }

        public async Task<List<ParticipantCommunicationDto>> GetParticipantsWithAssignmentsAsync(int eventId)
        {
            return await _repository.GetParticipantsWithAssignmentsAsync(eventId);
        }
        public async Task<bool> LogCardActionAsync(int eventId, int participantId, int userId, bool isPrintAction)
        {
            return await _repository.LogCardActionAsync(eventId, participantId, userId, isPrintAction);
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

                //emailRequest.FromEmail = "postmaster@sandboxdfa20f2294224a8cb8e81a8ecbb11738.mailgun.org";
                //emailRequest.ToEmails[0] = "bviraj44@gmail.com";
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
                // var qrData = $"EVENT:{eventId}|CODE:{participantCode}";
                var qrData = participantCode + "||" + eventId;
                
                // 1. Use raw text WITHOUT any prefix
               

                // 2. Generate QR code
                using var qrGenerator = new QRCodeGenerator();
                var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);

                // 3. Use PngByteQRCode (most reliable)
                var pngQrCode = new PngByteQRCode(qrCodeData);

                // 4. Generate with iPhone-friendly settings
                var pngBytes = pngQrCode.GetGraphic(
                    pixelsPerModule: 10,  // Optimal for iPhone
                    drawQuietZones: true //, // CRITICAL for iPhone
                                         //  quietZoneRendering: QRCoder.QRCodeGenerator.QuietZoneRendering.Flat
                );

                // 5. Convert to base64
                var base64 = Convert.ToBase64String(pngBytes);

                // 6. Test the QR code before returning
             //   TestQRCode(pngBytes, qrData);

                return $"data:image/png;base64,{base64}";


                //return $"data:image/png;base64,{qrCodeImageBase64}";
            }
            catch
            {
                return string.Empty;
            }
        }
        private void TestQRCode(byte[] pngBytes, string expectedText)
        {
            // Save to file for testing
            File.WriteAllBytes(@"C:\Users\Admin\source\repos\Images\test_qr1.png", pngBytes);
            Console.WriteLine($"QR code saved. Expected text: '{expectedText}'");
            Console.WriteLine("Scan this file with iPhone Camera app to test.");
        }
        public async Task<ScanResultDto> GenerateIdCardAsync(int eventId, int participantId)
        {
            try
            {
                // 1. Get email configuration
                var emailConfig = await _scanRepository.GetPassConfigurationAsync(eventId);

                // 2. Get participant data
                var participantData = await _repository.GetParticipantsDetailsAsync(eventId, participantId);

                // Check if participant data is null
                if (participantData == null)
                {
                    return new ScanResultDto
                    {
                        Success = false,
                        ValidationMessage = "Participant not found"
                    };
                }

                // 3. Generate QR Code
                var qrCodeBase64 = GenerateQRCode(participantData.ParticipantCode, eventId);

                // 4. Get ID card template from configuration - CORRECTED LINE
                // Template is in BodyText field, not IdCardTemplate
                string idCardTemplate = emailConfig?.BodyText?.ToString() ?? "";

                // 5. Replace placeholders in the ID card template
                string idCardHtml = ReplaceIdCardPlaceholders(idCardTemplate, participantData, qrCodeBase64);

                // 6. Create response with the generated ID card
                return new ScanResultDto
                {
                    Success = true,
                    IdCardHtml = idCardHtml,
                    Message = "ID card generated successfully",
                    ParticipantId = participantId,
                    FullName = participantData.FullName?.ToString() ?? "",
                    ParticipantCode = participantData.ParticipantCode?.ToString() ?? "",
                    ValidationStatus = "VALID",
                    ValidationMessage = "ID card generated",
                    Status = "Generated",
                    ScanTime = DateTime.Now,
                    IsPrintCenter = true
                };
            }
            catch (Exception ex)
            {
                return new ScanResultDto
                {
                    Success = false,
                    ValidationMessage = $"Failed to generate ID card: {ex.Message}"
                };
            }
        }

        private string ReplaceIdCardPlaceholders(string template, dynamic participant, string qrCodeBase64 = null)
        {
            if (string.IsNullOrEmpty(template))
                return "<div>No ID card template available</div>";

            var html = template
                .Replace("@EVENTNAME@", participant.EventName?.ToString() ?? "")
                .Replace("@EventDate@", participant.EventDate?.ToString() ?? "")  // Note: This needs to match template
                .Replace("@ParticipantName@", participant.FullName?.ToString() ?? "")
                .Replace("@Company@", participant.Company?.ToString() ?? "")
                .Replace("@Department@", participant.Department?.ToString() ?? "")
                .Replace("@ParticipantCode@", participant.ParticipantCode?.ToString() ?? "")
                .Replace("@Email@", participant.Email?.ToString() ?? "")
                .Replace("@Country@", participant.Country?.ToString() ?? "")
                ;

            // IMPORTANT: Replace @QR_BASE64@ with JUST the base64 string, not the whole img tag
            if (!string.IsNullOrEmpty(qrCodeBase64))
            {
                html = html.Replace("@QR_BASE64@", qrCodeBase64);
            }
            else
            {
                // If no QR code, use empty string
                html = html.Replace("@QR_BASE64@", "");
            }

            return html;
        }
    }
}