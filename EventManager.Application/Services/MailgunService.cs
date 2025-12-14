using EventManager.Application.DTOs;
using EventManager.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EventManager.Application.Services
{
    public class MailgunService : IMailgunService
    {
        public readonly MailgunSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<MailgunService> _logger;

        public MailgunService(IOptions<MailgunSettings> settings, HttpClient httpClient, ILogger<MailgunService> logger)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _logger = logger;

            // Configure HttpClient
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{_settings.ApiKey}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        }

        public async Task<EmailResponse> SendEmailAsync(EmailRequest emailRequest, List<EmailAttachment> attachments = null)
        {
            try
            {

                var options = new RestClientOptions("https://api.mailgun.net")
                {
                    Authenticator = new HttpBasicAuthenticator("api", "b8494c5f494ba7375db96e6f6f45adbc-04af4ed8-0d1ffb35")// "b48e4dcce1ff7d335e79f8e0fde4d4c9 -04af4ed8-0fccce24")
                };

                var client = new RestClient(options);
                var request = new RestRequest("/v3/sandbox9436a5ace7524a81a718b2e3dd399978.mailgun.org/messages", Method.Post);
                request.AlwaysMultipartFormData = true;
                request.AddParameter("from", "bviraj44@gmail.com");
                request.AddParameter("to", "Viraj <bviraj444@gmail.com>");
                request.AddParameter("subject", "Hello Viraj");
                request.AddParameter("text", "Congratulations Viraj, you just sent an email with Mailgun! You are truly awesome!");
                var resultVal = await client.ExecuteAsync(request);
                // Check if the request was successful
                if (!resultVal.IsSuccessful)
                {
                    _logger.LogError("RestSharp request failed: {ErrorMessage} - {StatusDescription}",
                        resultVal.ErrorMessage, resultVal.StatusDescription);

                    // Handle different error scenarios
                    return new EmailResponse
                    {
                        Success = false,
                        Error = resultVal.ErrorMessage ?? $"HTTP Error: {resultVal.StatusCode}"
                    };
                }

                // Ensure we have content
                if (string.IsNullOrWhiteSpace(resultVal.Content))
                {
                    _logger.LogError("Empty response received from Mailgun API");
                    return new EmailResponse
                    {
                        Success = false,
                        Error = "Empty response from email service"
                    };
                }

                // Now handle the response based on status code
                if (resultVal.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var mailgunResponse = JsonConvert.DeserializeObject<MailgunApiResponse>(resultVal.Content);

                    if (mailgunResponse == null)
                    {
                        _logger.LogError("Failed to deserialize Mailgun successful response");
                        return new EmailResponse
                        {
                            Success = false,
                            Error = "Failed to parse email service response"
                        };
                    }

                    _logger.LogInformation("Email sent successfully. Message ID: {MessageId}", mailgunResponse.Id);

                    return new EmailResponse
                    {
                        Success = true,
                        MessageId = mailgunResponse.Id,
                        Message = "Email sent successfully"
                    };
                }


                else
                {
                    // Handle non-200 status codes
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<MailgunErrorResponse>(resultVal.Content);
                        var errorMessage = errorResponse?.Message ?? resultVal.ErrorMessage ?? "Unknown error";

                        _logger.LogError("Mailgun API error: {StatusCode} - {Message}",
                            resultVal.StatusCode, errorMessage);

                        return new EmailResponse
                        {
                            Success = false,
                            Error = errorMessage
                        };
                    }
                    catch (Newtonsoft.Json.JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to parse Mailgun error response: {Content}",
                            resultVal.Content);

                        return new EmailResponse
                        {
                            Success = false,
                            Error = $"HTTP {(int)resultVal.StatusCode}: {resultVal.StatusCode}"
                        };
                    }
                }
                //// Validate request
                //if (emailRequest.ToEmails == null || !emailRequest.ToEmails.Any())
                //{
                //    return new EmailResponse
                //    {
                //        Success = false,
                //        Error = "No recipients specified"
                //    };
                //}

                //// Create form data
                //using var formData = new MultipartFormDataContent();

                //// Add sender
                //var from = string.IsNullOrEmpty(emailRequest.FromName) 
                //    ? emailRequest.FromEmail 
                //    : $"{emailRequest.FromName} <{emailRequest.FromEmail}>";
                //formData.Add(new StringContent(from), "from");

                //// Add recipients
                //formData.Add(new StringContent(string.Join(",", emailRequest.ToEmails)), "to");

                //if (emailRequest.CcEmails != null && emailRequest.CcEmails.Any())
                //    formData.Add(new StringContent(string.Join(",", emailRequest.CcEmails)), "cc");

                //if (emailRequest.BccEmails != null && emailRequest.BccEmails.Any())
                //    formData.Add(new StringContent(string.Join(",", emailRequest.BccEmails)), "bcc");

                //// Add subject and message
                //formData.Add(new StringContent(emailRequest.Subject), "subject");

                //if (emailRequest.IsHtml)
                //    formData.Add(new StringContent(emailRequest.Message), "html");
                //else
                //    formData.Add(new StringContent(emailRequest.Message), "text");

                //// Add custom variables
                //if (emailRequest.CustomVariables != null)
                //{
                //    foreach (var variable in emailRequest.CustomVariables)
                //    {
                //        formData.Add(new StringContent(variable.Value), $"v:{variable.Key}");
                //    }
                //}

                //// Add tag for tracking
                //if (!string.IsNullOrEmpty(emailRequest.Tag))
                //    formData.Add(new StringContent(emailRequest.Tag), "o:tag");

                //// Add tracking options
                //formData.Add(new StringContent("yes"), "o:tracking");
                //formData.Add(new StringContent("yes"), "o:tracking-clicks");
                //formData.Add(new StringContent("yes"), "o:tracking-opens");

                //// Add attachments
                //if (attachments != null)
                //{
                //    foreach (var attachment in attachments)
                //    {
                //        var fileContent = new ByteArrayContent(attachment.Content);
                //        fileContent.Headers.ContentType = new MediaTypeHeaderValue(attachment.ContentType);
                //        formData.Add(fileContent, "attachment", attachment.FileName);
                //    }
                //}

                //// Determine API endpoint based on region
                //var apiUrl = _settings.Region.ToLower() == "eu" 
                //    ? $"https://api.eu.mailgun.net/v3/{_settings.Domain}/messages"
                //    : $"https://api.mailgun.net/v3/{_settings.Domain}/messages";

                //// Send request
                //var response = await _httpClient.PostAsync(apiUrl, formData);
                //var responseContent = await response.Content.ReadAsStringAsync();

                // if (resultVal.ResponseStatus.suc..IsSuccessStatusCode)
                //if (resultVal.StatusCode == System.Net.HttpStatusCode.OK)
                //{
                //    var mailgunResponse = JsonConvert.DeserializeObject<MailgunApiResponse>(responseContent);

                //    _logger.LogInformation("Email sent successfully. Message ID: {MessageId}", mailgunResponse.Id);

                //    return new EmailResponse
                //    {
                //        Success = true,
                //        MessageId = mailgunResponse.Id,
                //        Message = "Email sent successfully"
                //    };
                //}
                //else
                //{
                //    var errorResponse = JsonConvert.DeserializeObject<MailgunErrorResponse>(responseContent);

                //    _logger.LogError("Mailgun API error: {StatusCode} - {Message}", response.StatusCode, errorResponse.Message);

                //    return new EmailResponse
                //    {
                //        Success = false,
                //        Error = errorResponse.Message
                //    };
                //}




            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email via Mailgun");

                return new EmailResponse
                {
                    Success = false,
                    Error = $"Failed to send email: {ex.Message}"
                };
            }
        }

        public async Task<bool> ValidateCredentialsAsync()
        {
            try
            {
                var apiUrl = _settings.Region.ToLower() == "eu"
                    ? $"https://api.eu.mailgun.net/v3/domains/{_settings.Domain}"
                    : $"https://api.mailgun.net/v3/domains/{_settings.Domain}";

                var response = await _httpClient.GetAsync(apiUrl);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        //public async Task<List<EmailEvent>> GetEmailEventsAsync(string messageId)
        //{
        //    try
        //    {
        //        var apiUrl = _settings.Region.ToLower() == "eu"
        //            ? $"https://api.eu.mailgun.net/v3/{_settings.Domain}/events?message-id={messageId}"
        //            : $"https://api.mailgun.net/v3/{_settings.Domain}/events?message-id={messageId}";

        //        var response = await _httpClient.GetAsync(apiUrl);
        //        var content = await response.Content.ReadAsStringAsync();

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var eventsResponse = JsonConvert.DeserializeObject<MailgunEventsResponse>(content);
        //            return eventsResponse.Items.Select(e => new EmailEvent
        //            {
        //                Event = e.Event,
        //                Timestamp = e.Timestamp,
        //                Recipient = e.Recipient,
        //                MessageId = e.Message?.Headers?["message-id"],
        //                Severity = e.Severity,
        //                Reason = e.Reason
        //            }).ToList();
        //        }

        //        return new List<EmailEvent>();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving email events");
        //        return new List<EmailEvent>();
        //    }
        //}

        public async Task<List<EmailEvent>> GetEmailEventsAsync(string messageId)
        {
            try
            {
                var apiUrl = _settings.Region.ToLower() == "eu"
                    ? $"https://api.eu.mailgun.net/v3/{_settings.Domain}/events"
                    : $"https://api.mailgun.net/v3/{_settings.Domain}/events";

                var url = $"{apiUrl}?message-id={Uri.EscapeDataString(messageId)}&limit=100";

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var events = new List<EmailEvent>();
                    var json = JObject.Parse(content);
                    var items = json["items"] as JArray;

                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            var emailEvent = new EmailEvent
                            {
                                Event = item["event"]?.ToString(),
                                Timestamp = EmailEvent.UnixTimeStampToDateTime(item["timestamp"]?.Value<double>() ?? 0),
                                Recipient = item["recipient"]?.ToString(),
                                MessageId = messageId,
                                Severity = item["severity"]?.ToString(),
                                Reason = item["reason"]?.ToString()
                            };

                            // Get message-id from headers using Value<string>() method
                            var messageIdToken = item["message"]?["headers"]?["message-id"];
                            if (messageIdToken != null)
                            {
                                emailEvent.MessageId = messageIdToken.Value<string>();
                            }

                            events.Add(emailEvent);
                        }
                    }

                    return events;
                }

                return new List<EmailEvent>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email events");
                return new List<EmailEvent>();
            }
        }
    }


    // API Response Models
    public class MailgunApiResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class MailgunErrorResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public class MailgunEventsResponse
    {
        [JsonProperty("items")]
        public List<EventItem> Items { get; set; }
    }

    public class EventItem
    {
        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("timestamp")]
        public double Timestamp { get; set; }

        [JsonProperty("recipient")]
        public string Recipient { get; set; }

        [JsonProperty("message")]
        public MessageDetails Message { get; set; }

        [JsonProperty("severity")]
        public string Severity { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }

    public class MessageDetails
    {
        [JsonProperty("headers")]
        public Headers Headers { get; set; }
    }

    public class Headers
    {
        [JsonProperty("message-id")]
        public string MessageId { get; set; }
    }
}
