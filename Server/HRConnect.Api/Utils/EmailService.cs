namespace HRConnect.Api.Utils
{
  using SendGrid;
  using SendGrid.Helpers.Mail;

  public interface IEmailService
  {
    Task SendEmailAsync(string recipientEmail, string subject, string body);
  }

  public partial class EmailService : IEmailService
  {
    private readonly SendGridClient _client;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(IConfiguration configuration)
    {
      string? sendGridApiKey = configuration["SendGrid:ApiKey"];
      _fromEmail = configuration["SendGrid:FromEmail"] ?? "ochimerema@gmail.com";
      _fromName = configuration["SendGrid:FromName"] ?? "HRConnect";

      if (string.IsNullOrWhiteSpace(sendGridApiKey))
      {
        throw new InvalidOperationException("SendGrid API key is not configured.");
      }

      _client = new SendGridClient(sendGridApiKey);
    }

    public async Task SendEmailAsync(string recipientEmail, string subject, string body)
    {
      if (string.IsNullOrWhiteSpace(recipientEmail))
        throw new ArgumentException("Recipient email is required.");

      if (string.IsNullOrWhiteSpace(subject))
        throw new ArgumentException("Email subject is required.");

      if (string.IsNullOrWhiteSpace(body))
        throw new ArgumentException("Email body is required.");

      var msg = new SendGridMessage()
      {
        From = new EmailAddress(_fromEmail, _fromName),
        Subject = subject,
        HtmlContent = body,
        PlainTextContent = StripHtml(body)
      };

      msg.AddTo(new EmailAddress(recipientEmail));

      var response = await _client.SendEmailAsync(msg);
      var responseBody = await response.Body.ReadAsStringAsync();

      Console.WriteLine($"Status: {response.StatusCode}");
      Console.WriteLine($"Body: {responseBody}");

      if (!response.IsSuccessStatusCode)
      {
        var errorBody = await response.Body.ReadAsStringAsync();
        throw new InvalidOperationException(
            $"Failed to send email to {recipientEmail}. StatusCode: {response.StatusCode}. Response: {errorBody}");
      }
    }
    // fallback for email clients that don't support HTML
    [System.Text.RegularExpressions.GeneratedRegex("<.*?>")]
    private static partial System.Text.RegularExpressions.Regex HtmlRegex();
    private static string StripHtml(string html)
    {
      return HtmlRegex().Replace(html, string.Empty);
    }
  }
}