using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using iText.Barcodes;
using iText.IO.Font;
using iText.IO.Font.Constants;      
using iText.Kernel.Colors;           
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Extensions.Configuration;
using QRCoder;
using System.Text;
using static EventManager.Application.DTOs.ScanDtos;
using Border = iText.Layout.Borders.Border;
using Document = iText.Layout.Document;


 

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
        public async Task<int> LogCardActionAsync(int eventId, int participantId, int userId, bool isPrintAction)
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
        public async Task<ScanResultDto> GenerateIdCardAsyncOld(int eventId, int participantId)
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


        public async Task<ScanResultDto> GenerateIdCardAsync(int eventId, int participantId,string fontFolder)
        {
            try
            {
                // 1. Get participant data
                var participantData = await _repository.GetParticipantsDetailsAsync(eventId, participantId);

                if (participantData == null)
                {
                    return new ScanResultDto { Success = false, ValidationMessage = "Participant not found" };
                }

                // 2. Prepare the combined string (This is what you want inside the QR)
                string qrData = participantData.ParticipantCode + "||" + eventId;

                // 3. Generate QR Code Base64 (For UI/Preview)
                var qrCodeBase64 = GenerateQRCode(participantData.ParticipantCode, eventId);

                // 4. Generate the PDF (PASSING qrData instead of just participantCode)
                byte[] pdfBytes = await GenerateIDCard(participantData, qrData,fontFolder);

                // 5. Create response
                return new ScanResultDto
                {
                    Success = true,
                    IdCardPdf = pdfBytes,
                    QrCodeBase64 = qrCodeBase64, // Added this so your frontend can use it
                    Message = "ID card generated successfully",
                    ParticipantId = participantId,
                    FullName = participantData.FullName ?? "",
                    ParticipantCode = participantData.ParticipantCode ?? "",
                    ValidationStatus = "VALID",
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

        public async Task<byte[]> GenerateIDCardLastWorking(dynamic participantData, string qrCodeValue, string fontFolder)
        {
            double pointsPerMm = 72d / 25.4d;
            PageSize a6Page = PageSize.A6;

            float contentWidth = (float)(101.6f * pointsPerMm);
            float contentHeight = (float)(65.5f * pointsPerMm);
            float qrSize = (float)(20f * pointsPerMm); // Increased to 20mm as requested

            float startY = (float)(2f * pointsPerMm);
            float startX = (a6Page.GetWidth() - contentWidth) / 2;

            PdfFont fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            PdfFont fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            string arialRegularPath = System.IO.Path.Combine(fontFolder, "ARIAL.ttf");
            string arialBoldPath = System.IO.Path.Combine(fontFolder, "ARIALBD.ttf");
            string arialBlackPath = System.IO.Path.Combine(fontFolder, "ariblk.ttf");

            PdfFont fontArialBlack = null;

            if (File.Exists(arialRegularPath))
                fontRegular = PdfFontFactory.CreateFont(arialRegularPath, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

            if (File.Exists(arialBoldPath))
                fontBold = PdfFontFactory.CreateFont(arialBoldPath, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

            if (File.Exists(arialBlackPath))
                fontArialBlack = PdfFontFactory.CreateFont(arialBlackPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

            float textMaxWidth = contentWidth - 10;

            using (MemoryStream pdfStream = new MemoryStream())
            {
                using (PdfWriter pdfWriter = new PdfWriter(pdfStream))
                using (PdfDocument pdfDocument = new PdfDocument(pdfWriter))
                {
                    pdfDocument.SetDefaultPageSize(a6Page);
                    Document document = new Document(pdfDocument);
                    document.SetMargins(0, 0, 0, 0);

                    // Main container using Flex layout for better space distribution
                    Div container = new Div()
                        .SetWidth(contentWidth)
                        .SetHeight(contentHeight)
                        .SetFixedPosition(startX, startY, contentWidth);

                    // --------TOP SECTION--------
                    Div topSection = new Div()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginTop(3) // Reduced top margin
                        .SetHeight(contentHeight * 0.35f); // Reduced to 35% (from 40%)

                    string fullName = participantData.FullName ?? "N/A";
                    float nameFontSize = GetFittedFontSize(fullName, fontBold, 24f, 14f, textMaxWidth);

                    // Adjust name paragraph margins
                    topSection.Add(new Paragraph(fullName)
                        .SetFont(fontBold)
                        .SetFontSize(nameFontSize)
                        .SetMargin(0)
                        .SetMarginBottom(1) // Reduced bottom margin
                        .SetMaxHeight(contentHeight * 0.25f));

                    // Reduce company paragraph margins significantly
                    topSection.Add(new Paragraph(participantData.Company ?? "")
                        .SetFont(fontRegular)
                        .SetFontSize(15) // Reduced from 15
                        .SetMarginTop(0) // Reduced from 2
                        .SetMarginBottom(1)); // Reduced from 5

                    // --------BOTTOM SECTION--------
                    Div bottomSection = new Div()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetHeight(contentHeight * 0.65f); // Increased to 65% (from 60%)

                    string country = participantData.Country ?? "";
                    float countryFontSize = GetFittedFontSize(country, fontRegular, 15f, 10f, textMaxWidth); // Reduced max from 15

                    // Country paragraph with minimal spacing
                    Paragraph countryParagraph = new Paragraph(country)
                        .SetFont(fontRegular)
                        .SetFontSize(countryFontSize)
                        .SetMarginTop(0) // No top margin
                        .SetMarginBottom(1); // Reduced from 5

                    bottomSection.Add(countryParagraph);

                    // QR Code with increased size
                    BarcodeQRCode qrCode = new BarcodeQRCode(qrCodeValue ?? "Empty");
                    Image qrImage = new Image(qrCode.CreateFormXObject(pdfDocument))
                        .SetWidth(qrSize)
                        .SetHeight(qrSize)
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                        .SetMarginTop(2) // Added small top margin
                        .SetMarginBottom(3); // Reduced from 5

                    bottomSection.Add(qrImage);

                    // Notes section with reduced maximum height
                    Div notesContainer = new Div()
                        .SetWidth(contentWidth)
                        .SetMaxHeight(contentHeight * 0.2f) // Reduced from 25% to 20%
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetMarginTop(2); // Added small top margin

                    string notes = participantData.Notes ?? "";
                    float notesAvailableWidth = textMaxWidth - 10;
                    float notesFontSize = GetFittedFontSize(notes, fontBold, 18f, 8f, notesAvailableWidth); // Reduced max from 20

                    // Additional check for vertical space
                    float maxNotesHeight = contentHeight * 0.2f; // Reduced from 25%
                    float lineHeight = notesFontSize * 1.2f;
                    int estimatedLines = (int)Math.Ceiling(notes.Length / 15f);

                    // Reduce font size if notes would take too many lines
                    if (estimatedLines * lineHeight > maxNotesHeight)
                    {
                        notesFontSize = maxNotesHeight / (estimatedLines * 1.2f);
                        notesFontSize = Math.Max(8f, notesFontSize);
                    }

                    Paragraph notesParagraph = new Paragraph(notes)
                        .SetFont(fontBold)
                        .SetFontSize(notesFontSize)
                        .SetFontColor(ColorConstants.BLACK)
                        .SetMargin(0)
                        .SetPadding(0)
                        .SetMaxHeight(maxNotesHeight);

                    notesContainer.Add(notesParagraph);
                    bottomSection.Add(notesContainer);

                    // Add sections to main container
                    container.Add(topSection);
                    container.Add(bottomSection);

                    document.Add(container);
                    document.Close();
                }

                return pdfStream.ToArray();
            }
        }
        public async Task<byte[]> GenerateIDCard(dynamic participantData, string qrCodeValue, string fontFolder)
        {
            double pointsPerMm = 72d / 25.4d;
            PageSize a6Page = PageSize.A6;

            float contentWidth = (float)(101.6f * pointsPerMm);
            float contentHeight = (float)(65.5f * pointsPerMm);
            float qrSize = (float)(22f * pointsPerMm);

            float startY = (float)(2f * pointsPerMm);
            float startX = (a6Page.GetWidth() - contentWidth) / 2;

            // ... font loading code remains the same ...

            PdfFont fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            PdfFont fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            string arialRegularPath = System.IO.Path.Combine(fontFolder, "ARIAL.ttf");
            string arialBoldPath = System.IO.Path.Combine(fontFolder, "ARIALBD.ttf");
            string arialBlackPath = System.IO.Path.Combine(fontFolder, "ariblk.ttf");

            PdfFont fontArialBlack = null;

            if (File.Exists(arialRegularPath))
                fontRegular = PdfFontFactory.CreateFont(arialRegularPath, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

            if (File.Exists(arialBoldPath))
                fontBold = PdfFontFactory.CreateFont(arialBoldPath, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

            if (File.Exists(arialBlackPath))
                fontArialBlack = PdfFontFactory.CreateFont(arialBlackPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);



            float textMaxWidth = contentWidth - 10;

            using (MemoryStream pdfStream = new MemoryStream())
            {
                using (PdfWriter pdfWriter = new PdfWriter(pdfStream))
                using (PdfDocument pdfDocument = new PdfDocument(pdfWriter))
                {
                    pdfDocument.SetDefaultPageSize(a6Page);
                    Document document = new Document(pdfDocument);
                    document.SetMargins(0, 0, 0, 0);

                    // -------- NAME SECTION (Fixed position at top) --------
                    string fullName = participantData.FullName ?? "N/A";
                    float nameFontSize = GetFittedFontSize(fullName, fontBold, 24f, 14f, textMaxWidth);

                    Paragraph namePara = new Paragraph(fullName)
                        .SetFont(fontBold)
                        .SetFontSize(nameFontSize)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetWidth(contentWidth)
                      //  .SetBorder(new SolidBorder(ColorConstants.RED, 1))
                        .SetFixedPosition(startX, startY + contentHeight - 45, contentWidth);
                        
                    document.Add(namePara);

                    // -------- COMPANY SECTION (Below name) --------
                    float companyY = startY + contentHeight - 28 - nameFontSize * 1.5f;
                    Paragraph companyPara = new Paragraph(participantData.Company ?? "")
                        .SetFont(fontRegular)
                        .SetFontSize(15)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetWidth(contentWidth)
                        .SetFixedPosition(startX, companyY, contentWidth);

                    document.Add(companyPara);

                    // -------- COUNTRY SECTION (Fixed position above QR) --------
                    string country = participantData.Country ?? "";
                    float countryFontSize = GetFittedFontSize(country, fontRegular, 15f, 10f, textMaxWidth);

                    float countryY = startY + qrSize + 40; // Position above QR code
                    Paragraph countryPara = new Paragraph(country)
                        .SetFont(fontRegular)
                        .SetFontSize(countryFontSize)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetWidth(contentWidth)
                        .SetFixedPosition(startX, countryY, contentWidth);

                    document.Add(countryPara);

                    // -------- QR CODE (Fixed position in middle) --------
                    BarcodeQRCode qrCode = new BarcodeQRCode(qrCodeValue ?? "Empty");
                    Image qrImage = new Image(qrCode.CreateFormXObject(pdfDocument))
                        .SetWidth(qrSize)
                        .SetHeight(qrSize)
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                        .SetFixedPosition(startX + (contentWidth - qrSize) / 2, startY + 35, qrSize);

                    document.Add(qrImage);

                    // -------- NOTES SECTION (Always at bottom, fixed position) --------
                    string notes = participantData.Notes ?? "";
                    if (!string.IsNullOrEmpty(notes))
                    {
                        float notesFontSize = 28f;

                        // Scale only for width, not height
                        float textWidthAt28pt = fontBold.GetWidth(notes, 28f);
                        if (textWidthAt28pt > textMaxWidth)
                        {
                            notesFontSize = 28f * (textMaxWidth / textWidthAt28pt);
                            notesFontSize = Math.Max(12f, notesFontSize);
                        }

                        // Fixed position at bottom
                        Paragraph notesPara = new Paragraph(notes)
                            .SetFont(fontBold)
                            .SetFontSize(notesFontSize)
                            .SetFontColor(ColorConstants.BLACK)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetWidth(contentWidth)
                            .SetFixedPosition(startX, startY + 1, contentWidth); // 5pt from bottom

                        document.Add(notesPara);
                    }

                    document.Close();
                }

                return pdfStream.ToArray();
            }
        }
        public async Task<byte[]> GenerateIDCardLast(dynamic participantData, string qrCodeValue, string fontFolder)
        {
            double pointsPerMm = 72d / 25.4d;
            PageSize a6Page = PageSize.A6;

            float contentWidth = (float)(101.6f * pointsPerMm);
            float contentHeight = (float)(65.5f * pointsPerMm);
            float qrSize = (float)(22f * pointsPerMm); // Reduced to make more space

            float startY = (float)(2f * pointsPerMm);
            float startX = (a6Page.GetWidth() - contentWidth) / 2;

            PdfFont fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            PdfFont fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            string arialRegularPath = System.IO.Path.Combine(fontFolder, "ARIAL.ttf");
            string arialBoldPath = System.IO.Path.Combine(fontFolder, "ARIALBD.ttf");
            string arialBlackPath = System.IO.Path.Combine(fontFolder, "ariblk.ttf");

            PdfFont fontArialBlack = null;

            if (File.Exists(arialRegularPath))
                fontRegular = PdfFontFactory.CreateFont(arialRegularPath, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

            if (File.Exists(arialBoldPath))
                fontBold = PdfFontFactory.CreateFont(arialBoldPath, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

            if (File.Exists(arialBlackPath))
                fontArialBlack = PdfFontFactory.CreateFont(arialBlackPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

            float textMaxWidth = contentWidth - 10;

            using (MemoryStream pdfStream = new MemoryStream())
            {
                using (PdfWriter pdfWriter = new PdfWriter(pdfStream))
                using (PdfDocument pdfDocument = new PdfDocument(pdfWriter))
                {
                    pdfDocument.SetDefaultPageSize(a6Page);
                    Document document = new Document(pdfDocument);
                    document.SetMargins(0, 0, 0, 0);

                    // Main container using Flex layout for better space distribution
                    Div container = new Div()
                        .SetWidth(contentWidth)
                        .SetHeight(contentHeight)
                        .SetFixedPosition(startX, startY, contentWidth);
                       

                    // --------TOP SECTION--------
                    Div topSection = new Div()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMarginTop(5)
                        .SetMaxHeight(contentHeight * 0.4f)
                        ; // Limit top section to 40% of height

                    string fullName = participantData.FullName ?? "N/A";
                    float nameFontSize = GetFittedFontSize(fullName, fontBold, 24f, 14f, textMaxWidth);

                    topSection.Add(new Paragraph(fullName)
                        .SetFont(fontBold)
                        .SetFontSize(nameFontSize)
                        .SetMargin(0)
                        .SetMaxHeight(contentHeight * 0.3f)); // Limit name height

                    topSection.Add(new Paragraph(participantData.Company ?? "")
                        .SetFont(fontRegular)
                        .SetFontSize(15)
                        .SetMarginTop(0)
                        .SetMarginBottom(0));

                    // --------BOTTOM SECTION--------
                    Div bottomSection = new Div()
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetMinHeight(contentHeight * 0.6f); // Ensure bottom section has enough space

                    string country = participantData.Country ?? "";
                    float countryFontSize = GetFittedFontSize(country, fontRegular, 15f, 10f, textMaxWidth);

                    bottomSection.Add(new Paragraph(country)
                        .SetFont(fontRegular)
                        .SetFontSize(countryFontSize)
                        .SetMarginTop(0)
                        .SetMarginBottom(2));

                    // QR Code
                    BarcodeQRCode qrCode = new BarcodeQRCode(qrCodeValue ?? "Empty");
                    Image qrImage = new Image(qrCode.CreateFormXObject(pdfDocument))
                        .SetWidth(qrSize)
                        .SetHeight(qrSize)
                        .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                        .SetMarginTop(0)
                        .SetMarginBottom(0);

                    bottomSection.Add(qrImage);

                    // Notes section with fixed height constraint
                    ////Div notesContainer = new Div()
                    ////    .SetWidth(contentWidth)
                    ////    .SetMaxHeight(contentHeight * 0.25f) // Limit notes to 25% of total height
                    ////    .SetTextAlignment(TextAlignment.CENTER)
                    ////    .SetVerticalAlignment(VerticalAlignment.BOTTOM);

                    ////string notes = participantData.Notes ?? "";

                    ////// Calculate available width for notes (accounting for padding)
                    float notesAvailableWidth = textMaxWidth - 10;

                    ////// More aggressive font sizing for notes
                    //////float notesFontSize = GetFittedFontSize(notes, fontBold, 28f, 10f, notesAvailableWidth);
                    ////float notesFontSize = 28f;

                    ////// Only scale down if text is too wide for 28pt
                    ////float textWidthAt28pt = fontBold.GetWidth(notes, 28f);
                    ////if (textWidthAt28pt > notesAvailableWidth)
                    ////{
                    ////    // Scale proportionally to fit
                    ////    notesFontSize = 28f * (notesAvailableWidth / textWidthAt28pt);
                    ////    // Ensure minimum size
                    ////    notesFontSize = Math.Max(10f, notesFontSize);
                    ////}

                    ////// Additional check for vertical space
                    ////float maxNotesHeight = contentHeight * 0.25f; // 25% of total height
                    ////float lineHeight = notesFontSize * 1.2f; // Approximate line height
                    ////int estimatedLines = (int)Math.Ceiling(notes.Length / 15f); // Estimate ~15 chars per line

                    ////// Reduce font size if notes would take too many lines
                    ////if (estimatedLines * lineHeight > maxNotesHeight)
                    ////{
                    ////    notesFontSize = maxNotesHeight / (estimatedLines * 1.2f);
                    ////    notesFontSize = Math.Max(8f, notesFontSize); // Minimum 8pt
                    ////}

                    ////Paragraph notesParagraph = new Paragraph(notes)
                    ////    .SetFont(fontBold)
                    ////    .SetFontSize(notesFontSize)
                    ////    .SetFontColor(ColorConstants.BLACK)
                    ////    .SetMargin(0)
                    ////    .SetPadding(0)
                    ////    .SetMaxHeight(maxNotesHeight)
                    ////    ; // Shrink content if it overflows

                    ////notesContainer.Add(notesParagraph);
                    ///
                    // In your original code, modify just the notes section:

                    // Remove or adjust the maxHeight constraint:
                    Div notesContainer = new Div()
                        .SetWidth(contentWidth)
                        // Remove or increase this line:
                        // .SetMaxHeight(contentHeight * 0.25f)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.BOTTOM);

                    string notes = participantData.Notes ?? "";
                    float notesFontSize = 28f;

                    // Only scale for width, not height
                    float textWidthAt28pt = fontBold.GetWidth(notes, 28f);
                    if (textWidthAt28pt > notesAvailableWidth)
                    {
                        notesFontSize = 28f * (notesAvailableWidth / textWidthAt28pt);
                        notesFontSize = Math.Max(12f, notesFontSize); // Minimum 12pt
                    }

                    Paragraph notesParagraph = new Paragraph(notes)
                        .SetFont(fontBold)
                        .SetFontSize(notesFontSize)
                        .SetFontColor(ColorConstants.BLACK)
                        .SetMargin(0)
                        .SetPadding(0);
                    // Remove height constraint:
                    // .SetMaxHeight(maxNotesHeight)

                    notesContainer.Add(notesParagraph);
                    bottomSection.Add(notesContainer);

                    // Add sections to main container
                    container.Add(topSection);
                    container.Add(bottomSection);

                    document.Add(container);
                    document.Close();
                }

                return pdfStream.ToArray();
            }
        }

        // Helper method to calculate optimal font size
        private float GetFittedFontSize(string text, PdfFont font, float maxSize, float minSize, float maxWidth)
        {
            if (string.IsNullOrEmpty(text))
                return minSize;

            float fontSize = maxSize;

            while (fontSize > minSize)
            {
                float textWidth = font.GetWidth(text, fontSize);
                if (textWidth <= maxWidth)
                    break;
                fontSize -= 0.5f; // Reduce gradually
            }

            return Math.Max(fontSize, minSize);
        }


        //    public async Task<byte[]> GenerateIDCard(dynamic participantData, string qrCodeValue, string fontFolder)
        //    {
        //        double pointsPerMm = 72d / 25.4d;
        //        PageSize a6Page = PageSize.A6;

        //        float contentWidth = (float)(101.6f * pointsPerMm);
        //        float contentHeight = (float)(65.5f * pointsPerMm);
        //        float qrSize = (float)(20f * pointsPerMm);

        //        float startY = (float)(2f * pointsPerMm);
        //        float startX = (a6Page.GetWidth() - contentWidth) / 2;

        //        PdfFont fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        //        PdfFont fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        //        string arialRegularPath = System.IO.Path.Combine(fontFolder, "ARIAL.ttf");
        //        string arialBoldPath = System.IO.Path.Combine(fontFolder, "ARIALBD.ttf");
        //        string arialBlackPath = System.IO.Path.Combine(fontFolder, "ariblk.ttf");

        //        PdfFont fontArialBlack = null;

        //        if (File.Exists(arialRegularPath))
        //            fontRegular = PdfFontFactory.CreateFont(arialRegularPath, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

        //        if (File.Exists(arialBoldPath))
        //            fontBold = PdfFontFactory.CreateFont(arialBoldPath, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

        //        if (File.Exists(arialBlackPath))
        //            fontArialBlack = PdfFontFactory.CreateFont(arialBlackPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);

        //        float textMaxWidth = contentWidth - 10;

        //        using (MemoryStream pdfStream = new MemoryStream())
        //        {
        //            using (PdfWriter pdfWriter = new PdfWriter(pdfStream))
        //            using (PdfDocument pdfDocument = new PdfDocument(pdfWriter))
        //            {
        //                pdfDocument.SetDefaultPageSize(a6Page);
        //                Document document = new Document(pdfDocument);
        //                document.SetMargins(0, 0, 0, 0);

        //                Div container = new Div()
        //                    .SetWidth(contentWidth)
        //                    .SetHeight(contentHeight)
        //                    .SetFixedPosition(startX, startY, contentWidth);

        //                Table layoutTable = new Table(1)
        //                    .SetWidth(contentWidth)
        //                    .SetHeight(contentHeight);

        //                //   --------TOP SECTION--------
        //                Div topText = new Div()
        //                    .SetTextAlignment(TextAlignment.CENTER)
        //                    .SetPaddingTop(3);

        //                string fullName = participantData.FullName ?? "N/A";
        //                float nameFontSize = GetFittedFontSize(fullName, fontBold, 24f, 14f, textMaxWidth);

        //                topText.Add(new Paragraph(fullName)
        //                    .SetFont(fontBold)
        //                    .SetFontSize(nameFontSize)
        //                    .SetMargin(0));

        //                topText.Add(new Paragraph(participantData.Company ?? "")
        //                    .SetFont(fontRegular)
        //                    .SetFontSize(15)
        //                    .SetMarginTop(0)
        //                    .SetMarginBottom(0)
        //                    .SetMarginTop(0));

        //                // --------BOTTOM SECTION--------
        //                Div bottomArea = new Div()
        //                    .SetTextAlignment(TextAlignment.CENTER)
        //                    .SetPaddingBottom(0);

        //                string country = participantData.Country ?? "";
        //                float countryFontSize = GetFittedFontSize(country, fontRegular, 15f, 10f, textMaxWidth);

        //                bottomArea.Add(new Paragraph(country)
        //                    .SetFont(fontRegular)
        //                    .SetFontSize(countryFontSize)
        //                    .SetMarginBottom(0)
        //                    .SetMarginTop(0));

        //                BarcodeQRCode qrCode = new BarcodeQRCode(qrCodeValue ?? "Empty");
        //                Image qrImage = new Image(qrCode.CreateFormXObject(pdfDocument))
        //                    .SetWidth(qrSize)
        //                    .SetHeight(qrSize)
        //                    .SetHorizontalAlignment(HorizontalAlignment.CENTER);


        //                bottomArea.Add(qrImage);

        //                string notes = participantData.Notes ?? "";
        //                float notesFontSize = GetFittedFontSize(notes, fontBold, 28f, 16f, textMaxWidth);

        //                bottomArea.Add(new Paragraph(notes)
        //                    .SetFont(fontBold)
        //                    .SetFontSize(notesFontSize)
        //                    .SetFontColor(ColorConstants.BLACK)
        //                    .SetMargin(0)
        //                    .SetPadding(0));

        //                layoutTable.AddCell(new Cell()
        //                    .Add(topText)
        //                    .SetBorder(Border.NO_BORDER)
        //                    .SetVerticalAlignment(VerticalAlignment.MIDDLE));

        //                layoutTable.AddCell(new Cell()
        //                    .Add(bottomArea)
        //                    .SetBorder(Border.NO_BORDER)
        //                    .SetVerticalAlignment(VerticalAlignment.BOTTOM));

        //                container.Add(layoutTable);
        //                document.Add(container);
        //                document.Close();
        //            }

        //            return pdfStream.ToArray();
        //        }
        //    }
        //    private float GetFittedFontSize(
        //string text,
        //PdfFont font,
        //float maxFontSize,
        //float minFontSize,
        //float maxWidth)
        //    {
        //        if (string.IsNullOrWhiteSpace(text))
        //            return maxFontSize;

        //        float fontSize = maxFontSize;

        //        // First, check if text fits at max size
        //        float textWidth = font.GetWidth(text, fontSize);
        //        if (textWidth <= maxWidth)
        //        {
        //            // For very short text, slightly increase font size to fill space
        //            while (textWidth < maxWidth * 0.7f && fontSize < maxFontSize)
        //            {
        //                fontSize += 0.5f;
        //                textWidth = font.GetWidth(text, fontSize);
        //            }
        //            return fontSize;
        //        }

        //        // For long text, reduce font until it fits
        //        while (textWidth > maxWidth && fontSize > minFontSize)
        //        {
        //            fontSize -= 0.5f;
        //            textWidth = font.GetWidth(text, fontSize);
        //        }

        //        return fontSize;
        //    }

        //    private float GetFittedFontSize(
        //string text,
        //PdfFont font,
        //float maxFontSize,
        //float minFontSize,
        //float maxWidth)
        //    {
        //        if (string.IsNullOrWhiteSpace(text))
        //            return maxFontSize;

        //        float fontSize = maxFontSize;

        //        while (fontSize > minFontSize)
        //        {
        //            if (font.GetWidth(text, fontSize) <= maxWidth)
        //                break;

        //            fontSize -= 0.5f;
        //        }

        //        return fontSize;
        //    }

        ////public async Task<byte[]> GenerateIDCard(dynamic participantData, string qrCodeValue,string fontFolder)
        ////{
        ////    double pointsPerMm = 72d / 25.4d;
        ////    PageSize a6Page = PageSize.A6;

        ////    float contentWidth = (float)(101.6f * pointsPerMm);
        ////    float contentHeight = (float)(66f * pointsPerMm);
        ////    float qrSize = (float)(20f * pointsPerMm);

        ////    // Position of the main box
        ////    float startY = (float)(05f * pointsPerMm);
        ////    float startX = (a6Page.GetWidth() - contentWidth) / 2;

        ////    PdfFont fontBold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        ////    PdfFont fontRegular = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        ////    string arialRegularPath = System.IO.Path.Combine(fontFolder, "ARIAL.ttf");
        ////    string arialBoldPath = System.IO.Path.Combine(fontFolder, "ARIALBD.ttf");
        ////    string arialBlackPath = System.IO.Path.Combine(fontFolder, "ARIBLK.ttf");             

        ////    PdfFont fontarialblack = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        ////    if (File.Exists(arialRegularPath))
        ////    {
        ////        fontRegular = PdfFontFactory.CreateFont(arialRegularPath, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
        ////    }

        ////    if (File.Exists(arialRegularPath))
        ////    {
        ////        fontBold = PdfFontFactory.CreateFont(arialBoldPath, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
        ////    }
        ////    if (File.Exists(arialBlackPath))
        ////    {
        ////        fontarialblack = PdfFontFactory.CreateFont(arialBlackPath, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
        ////    }


        ////    // Create Arial Black font
        ////    // PdfFont fontArialBlack = PdfFontFactory.CreateFont("Fonts/ARIALBD.TTF", PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
        ////    // Note: You need to have Arial Black font file (ARIALBD.TTF) in your Fonts folder
        ////    // Alternatively, you can use another approach if you don't have the font file

        ////    using (MemoryStream pdfStream = new MemoryStream())
        ////    {
        ////        using (PdfWriter pdfWriter = new PdfWriter(pdfStream))
        ////        using (PdfDocument pdfDocument = new PdfDocument(pdfWriter))
        ////        {
        ////            pdfDocument.SetDefaultPageSize(a6Page);
        ////            Document document = new Document(pdfDocument);
        ////            document.SetMargins(0, 0, 0, 0);

        ////            // --- 1. THE MAIN CONTENT BOX ---
        ////            Div container = new Div()
        ////                .SetWidth(contentWidth)
        ////                .SetHeight(contentHeight)
        ////                .SetFixedPosition(startX, startY, contentWidth);
        ////                //.SetBorder(new SolidBorder(ColorConstants.BLACK, 1f));

        ////            Table layoutTable = new Table(1).SetWidth(contentWidth).SetHeight(contentHeight);

        ////            // Top Section: Name & Company - Reduced spacing
        ////            Div topText = new Div().SetTextAlignment(TextAlignment.CENTER).SetPaddingTop(5);
        ////            topText.Add(new Paragraph(participantData.FullName ?? "N/A")
        ////                .SetFont(fontBold).SetFontSize(24).SetMargin(0).SetMultipliedLeading(1.0f));
        ////            topText.Add(new Paragraph(participantData.Company ?? "")
        ////                .SetFont(fontRegular).SetFontSize(15).SetMargin(0).SetMarginTop(-2)); // Negative margin to reduce space

        ////            // Bottom Section: Country, QR, and "Delegate" text
        ////            Div bottomArea = new Div()
        ////                .SetTextAlignment(TextAlignment.CENTER)
        ////                .SetPaddingBottom(2);

        ////            // Country - moved closer to the top
        ////            bottomArea.Add(new Paragraph(participantData.Country ?? "")
        ////                .SetFont(fontRegular).SetFontSize(15).SetMarginBottom(5).SetMarginTop(0));

        ////            // Generate QR Code
        ////            BarcodeQRCode qrCode = new BarcodeQRCode(qrCodeValue ?? "Empty");
        ////            Image qrImage = new Image(qrCode.CreateFormXObject(pdfDocument))
        ////                .SetWidth(qrSize).SetHeight(qrSize)
        ////                .SetHorizontalAlignment(HorizontalAlignment.CENTER)
        ////                .SetMarginBottom(0); // Reduced margin below QR

        ////            bottomArea.Add(qrImage);

        ////            // Add "Delegate" text below QR with Arial Black 28pt
        ////            // Note: If Arial Black font is not available, fall back to Helvetica Bold
        ////            try
        ////            {
        ////                bottomArea.Add(new Paragraph(participantData.Notes ?? "")
        ////                    .SetFont(fontBold)
        ////                    .SetFontSize(28)
        ////                    .SetFontColor(ColorConstants.BLACK)
        ////                    .SetMargin(0)
        ////                    .SetPadding(0));
        ////            }
        ////            catch
        ////            {
        ////                // Fallback if Arial Black is not available
        ////                bottomArea.Add(new Paragraph(participantData.Notes ?? "")
        ////                    .SetFont(fontarialblack)
        ////                    .SetFontSize(28)
        ////                    .SetFontColor(ColorConstants.BLACK)
        ////                    .SetMargin(0)
        ////                    .SetPadding(0));
        ////            }

        ////            // ADD CELLS CORRECTLY
        ////            layoutTable.AddCell(
        ////                new Cell().Add(topText)
        ////                          .SetBorder(Border.NO_BORDER)
        ////                          .SetVerticalAlignment(VerticalAlignment.TOP)
        ////                          .SetPaddingBottom(0) // Reduced padding
        ////            );

        ////            layoutTable.AddCell(
        ////                new Cell().Add(bottomArea)
        ////                          .SetBorder(Border.NO_BORDER)
        ////                          .SetVerticalAlignment(VerticalAlignment.BOTTOM)
        ////                          .SetPaddingBottom(2)
        ////                          .SetPaddingTop(0) // Remove top padding to bring content closer
        ////            );

        ////            container.Add(layoutTable);
        ////            document.Add(container);
        ////            document.Close();
        ////        }
        ////        return pdfStream.ToArray();
        ////    }
        ////}
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
                .Replace("@Notes@", participant.Notes?.ToString() ?? "")
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