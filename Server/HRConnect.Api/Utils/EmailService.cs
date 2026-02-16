namespace HRConnect.Api.Utils
{
  using SendGrid;
  using SendGrid.Helpers.Mail;
  using Resend;

  public interface IEmailService
  {
    Task SendEmailAsync(string recipientEmail, string subject, string body);
  }

  public class EmailService : IEmailService
  {
    private readonly ResendClient _resendClient;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(ResendClient resendClient, IConfiguration configuration)
    {
      _resendClient = resendClient;
      _fromEmail = configuration["Resend:FromEmail"] ?? "onboarding@resend.dev";
      _fromName = configuration["Resend:FromName"] ?? "HRConnect";
    }

    public async Task SendEmailAsync(string recipientEmail, string subject, string body)
    {
      var message = new EmailMessage
      {
        From = $"{_fromName} <{_fromEmail}>",
        To = new[] { recipientEmail },
        Subject = subject,
        HtmlBody = body
      };

      try
      {
        var response = await _resendClient.EmailSendAsync(message);
        // If we got here without exception, email was sent successfully
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException(
          $"Failed to send email to {recipientEmail}.", ex);
      }
    }
  }
}
