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
    private readonly SendGridClient _client;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration)
    {
      string apiKey = configuration["SendGrid:ApiKey"];
      _client = new SendGridClient(apiKey);
      _fromEmail = configuration["SendGrid:FromEmail"] ?? "matshidzejanet@gmail.com";
      _fromName = configuration["SendGrid:FromName"] ?? "Janet";
    }

    public async Task SendEmailAsync(string recipientEmail, string subject, string body)
    {
        var msg = new SendGridMessage()
        {
            From = new EmailAddress(_fromEmail, _fromName),
            Subject = subject,
            HtmlContent = body
        };
        msg.AddTo(new EmailAddress(recipientEmail));

        var response = await _client.SendEmailAsync(msg);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to send email to {recipientEmail}. StatusCode: {response.StatusCode}");
        }
    }
  }
}
