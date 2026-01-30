namespace HRConnect.Api.Utils
{
  using SendGrid;
  using SendGrid.Helpers.Mail;

  public interface IEmailService
  {
    Task SendEmailAsync(string recipientEmail, string subject, string body);
  }

  public class EmailService : IEmailService
  {
    private readonly SendGridClient _sendGridClient;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration)
    {
      string? sendGridApiKey = configuration["SendGrid:ApiKey"];
      _fromEmail = configuration["SendGrid:FromEmail"] ?? "noreply@hrconnect.com";
      _fromName = configuration["SendGrid:FromName"] ?? "HRConnect";

      if (string.IsNullOrWhiteSpace(sendGridApiKey))
      {
        throw new InvalidOperationException("SendGrid API key is not configured.");
      }

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
