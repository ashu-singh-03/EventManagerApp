namespace EventManager.Application.DTOs
{
    public class EmailRequest
    {
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public List<string> ToEmails { get; set; }
        public List<string> CcEmails { get; set; }
        public List<string> BccEmails { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool IsHtml { get; set; } = true;
        public Dictionary<string, string> CustomVariables { get; set; }
        public string Tag { get; set; }
    }

    public class EmailAttachment
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }

    public class EmailResponse
    {
        public bool Success { get; set; }
        public string MessageId { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
    }

    public class MailgunSettings
    {
        public string ApiKey { get; set; }
        public string Domain { get; set; }
        public string BaseUrl { get; set; }
        public string Region { get; set; } = "us";
    }
    public class EmailEvent
    {
        public string Event { get; set; } // delivered, opened, clicked, etc.
        public DateTime Timestamp { get; set; }
        public string Recipient { get; set; }
        public string MessageId { get; set; }
        public string Severity { get; set; }
        public string Reason { get; set; }

        // Convert Unix timestamp to DateTime
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }

}
