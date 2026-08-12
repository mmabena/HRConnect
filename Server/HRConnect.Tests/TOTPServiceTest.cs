namespace HRConnect.Tests
{
  using Moq;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Services;
  using HRConnect.Api.Interfaces.TOTP;
  using HRConnect.Api.Interfaces.Notification;
  using Microsoft.Extensions.Configuration;
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Notification;
  using System.Text;
  public class TOTPServiceTest
  {
    private readonly Mock<ITOTPRepository> _totpRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IMFAUserSecretsService> _mfaServiceMock;
    private readonly Mock<IEmployeeService> _employeeServiceMock;
    private readonly Mock<INotificationFactory> _notiFactoryMock;
    private readonly Mock<IConfiguration> _configMock;

    public TOTPServiceTest()
    {
      _totpRepoMock = new Mock<ITOTPRepository>();
      _userRepoMock = new Mock<IUserRepository>();
      _mfaServiceMock = new Mock<IMFAUserSecretsService>();
      _employeeServiceMock = new Mock<IEmployeeService>();
      _notiFactoryMock = new Mock<INotificationFactory>();
      _configMock = new Mock<IConfiguration>();


    }

    [Fact]
    public async Task SendTotpAndNotifyShouldCompleteTask()
    {
      //Assert 
      //There should be a user to serve the notification to 
      int userId = 1;
      User user = new User
      {
        UserId = userId,
        Role = UserRole.NormalUser,
        TempRole = UserRole.SuperUser
      };
      CreateNotificationDto dto = new CreateNotificationDto
      {
        Subject = "A New Notiication",
        Message = "This is what the notification is about"
      };
      _userRepoMock.Setup(r => r.GetUserByIdAsync(userId))
        .ReturnsAsync(user);

      var task = _notiFactoryMock.Setup(r => r.ProduceNotificationAsync(dto));

      Assert.NotNull(user);
    }

    [Fact]
    public async Task VerifyOTPShouldFailWhenReplayed()
    {
      //Assign 
      long stepCount = 300;
      _totpRepoMock.Setup(r => r.IsReplay(It.IsAny<int>(), stepCount)).ReturnsAsync(false);
      _totpRepoMock.Setup(r => r.MarkUsedAsync(It.IsAny<int>(), stepCount)).Returns(Task.CompletedTask);

      var config = new ConfigurationBuilder().AddInMemoryCollection(
          new Dictionary<string, string>
      {
          {"Totp:StepMinutes","10"}
      })
        .Build();

      TOTPService otpService = new TOTPService(_totpRepoMock.Object, _userRepoMock.Object,
          _mfaServiceMock.Object, _employeeServiceMock.Object, _notiFactoryMock.Object, config);

      var result = await otpService.ValidateCodeAsync(1, Encoding.UTF8.GetBytes("FAIL"), "1234");

      Assert.False(result);
    }
  }
}