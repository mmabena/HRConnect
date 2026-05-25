namespace HRConnect.Api.Utils.Notification.Channels
{
  using HRConnect.Api.DTOs.Employee;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Models;

  public class EmailDeliveryChannel : INotificationDeliveryChannel
  {
    public string Name => "Email Delivery";
    public DeliveryChannel Channel => DeliveryChannel.Email;
    private readonly IEmailService _emailService;
    private readonly IEmployeeService _employeeService;
    public EmailDeliveryChannel(IEmailService emailService, IEmployeeService employeeService)
    {
      _emailService = emailService;
      _employeeService = employeeService;
    }
    public async Task SendNotificationAsync(Notification notification)
    {
      EmployeeDto? employeeDto = await _employeeService.GetEmployeeByIdAsync(notification.EmployeeId);
      if (employeeDto == null)
        throw new InvalidDataException($"Employee {notification.EmployeeId} Cannot Be Found");
      try
      {

        await _emailService.SendEmailAsync(employeeDto.Email, notification.Subject, notification.Message);

        //After sending an email we should probably mark it as read
      }
      catch (InvalidOperationException ex)
      {
        throw new InvalidOperationException($"Failed To Send Email {ex?.Message}");
      }
    }
  }
}