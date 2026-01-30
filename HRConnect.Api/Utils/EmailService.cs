namespace HRConnect.Api.Utils
{
    using SendGrid;
    using SendGrid.Helpers.Mail;
    using Microsoft.Extensions.Options;
    using HRConnect.Api.Settings;
    public interface IEmailService
    {
        Task SendEmailAsync(string recipientEmail, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly SendGridClient _sendGridClient;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IOptions<SendGridSettings> options)
        {
            var settings = options?.Value ?? throw new ArgumentNullException(nameof(options));

            var sendGridApiKey = settings.ApiKey;
            _fromEmail = string.IsNullOrWhiteSpace(settings.FromEmail) ? "noreply@hrconnect.com" : settings.FromEmail;
            _fromName = string.IsNullOrWhiteSpace(settings.FromName) ? "HRConnect" : settings.FromName;

            if (string.IsNullOrWhiteSpace(sendGridApiKey))
                throw new InvalidOperationException("SendGrid API key is not configured.");

            _sendGridClient = new SendGridClient(sendGridApiKey);
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var toEmail = new EmailAddress(recipientEmail); 
            var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, body, body);

            var response = await _sendGridClient.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Failed to send email to {recipientEmail}. StatusCode: {response.StatusCode}"); 
            }
        }
    }
}
