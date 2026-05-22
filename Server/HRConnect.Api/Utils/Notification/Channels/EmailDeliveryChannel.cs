namespace HRConnect.Api.Utils.Notification.Channels
{
  using System.Linq.Expressions;
  using System.Runtime.CompilerServices;
  using HRConnect.Api.DTOs.Employee;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Notification;
  using HRConnect.Api.Models;
  using NetTopologySuite.Geometries.Utilities;

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
        Console.ForegroundColor = ConsoleColor.Cyan;

        // await _emailService.SendEmailAsync(employeeDto.Email, notification.Subject, notification.Message);

#line 33 "EmailDeliverySerivce.cs"
        Console.WriteLine($"SENT EMAIL TO ${notification.EmployeeId}:{employeeDto.Email} SAYING {notification.Message}");
#line default
        Console.ResetColor();

        //After sending an email we should probably mark it as read
      }
      catch (InvalidOperationException ex)
      {
        throw new InvalidOperationException($"Failed To Send Email {ex?.Message}");
      }
    }
  }
}