namespace HRConnect.Api.Services
{
  using OtpNet;
  using Microsoft.Extensions.Configuration;
  using HRConnect.Api.Interfaces.TOTP;
  using HRConnect.Api.DTOs.Employee;
  using HRConnect.Api.Models;
  using HRConnect.Api.Interfaces;
  using Microsoft.EntityFrameworkCore.Design;
  using HRConnect.Api.DTOs.Notification;
  using HRConnect.Api.Interfaces.Notification;
  using System.Threading.Tasks;

  /// <remarks>
  /// * IUserRepository has been injected as a dependency to. 
  /// * This is done to avoid circular dependency injection.
  /// * IUserService -> ITOTPService thus we CANNOT have it that
  /// * ITOTPService -> TUserService and so we arrive at
  /// * TOTPService -> IUserRepository 
  /// </remarks>

  public class TOTPService : ITOTPService
  {
    private readonly ITOTPRepository _totpRepo;
    private readonly IUserRepository _userRepo;
    private readonly IMFAUserSecretsService _mfaService;
    private readonly IEmployeeService _employeeService;
    private readonly INotificationFactory _notiFactory;
    private readonly int _stepSeconds;

    public TOTPService(ITOTPRepository totpRepo, IUserRepository userRepo, IMFAUserSecretsService mfaService, IEmployeeService employeeService,
        INotificationFactory notiFactory, IConfiguration configuration)
    {
      _totpRepo = totpRepo;
      _employeeService = employeeService;
      _stepSeconds = ResolveStepDuration(configuration);
      _userRepo = userRepo;
      _mfaService = mfaService;
      _notiFactory = notiFactory;
    }
    ///<summary>
    /// Method has mulitple related responsibilities for sending Time-Base
    /// One-Time-Pin. <see cref="MFAUserSecretsService.GetOrCreateUserSecretAsync(int)"
    ///is used to create user secret of which the pin is based off of.. 
    ///
    ///<remarks><a href="datatracker.ietf.org/doc/html/rfc6238">
    /// See RFC6238 for algorithm details and recommended implementations
    /// </a>
    /// </remarks>
    public async Task SendTotpAndNotify(int userId)
    {
      try
      {
        User? user = await _userRepo.GetUserByIdAsync(userId);
        if (user == null)
          throw new KeyNotFoundException();

        byte[] secret = await _mfaService.GetOrCreateUserSecretAsync(user.UserId);
        string code = GenerateCode(secret);

        var inAppNotification = await MakeInAppNotification(userId);
        var emailNotification = await MakeEmailNotification(userId, code);

        await _notiFactory.ProduceNotificationAsync(inAppNotification);
        await _notiFactory.ProduceNotificationAsync(emailNotification);
      }
      catch (OperationException ex)
      {
        throw new OperationException($"Failed To Send OTP {ex.Message}");
      }
    }
    public string GenerateCode(byte[] userSecret)
    {
      Totp otpCode = new(userSecret, step: _stepSeconds, OtpHashMode.Sha256);
      return otpCode.ComputeTotp();
    }
    public async Task<bool> ValidateCodeAsync(int userId, byte[] userSecret, string code)
    {
      Totp otpCode = new(userSecret, step: _stepSeconds, OtpHashMode.Sha256);

      //TOTP are generated every 10 minutes (size of out step),
      // step size == Step(Minutes/Seconds)
      bool isValid = otpCode.VerifyTotp(code, out long timeStepMatched,
      // VerificationWindow.previous=1 step back (10 minutes back) 
      // VerificationWindow.future=1 step forward (10 minutes ahead) 
        new VerificationWindow(previous: 1, future: 1));

      if (await IsReplayAsync(userId, timeStepMatched))
        return false;

      if (!isValid)
        return false;


      await MarkUsedCodeAsync(userId, timeStepMatched);
      return true;
    }
    /// <summary>
    ///  A replay store is used to check where a pin code has been previously used 
    /// before to prevent a basic briute force replay attack
    /// </summary>
    /// <param name="userId">UserSecret To check against</param>
    /// <param name="stepCount">The time step point the pin should be alive until
    /// </param>
    /// <returns>truthy value if the pin has been used</returns>
    public async Task<bool> IsReplayAsync(int userId, long stepCount)
    {
      return await _totpRepo.IsReplay(userId, stepCount);
    }

    public async Task MarkUsedCodeAsync(int userId, long stepCount)
    {
      await _totpRepo.MarkUsedAsync(userId, stepCount);
    }

    ///<summary>
    /// A helper function to set and resolve the Step Duration (or lifetime)
    /// the Time-Based One-Time-Pin
    ///</summary>
    private int ResolveStepDuration(IConfiguration configuration)
    {
      int minutes = configuration.GetValue("Totp:StepMinutes", 1);
      if (minutes <= 0) minutes = 10;

      int seconds = Math.Max(minutes, 1) * 60;
      if (seconds <= 0) return seconds * 600;

      return 30;
    }

    private async Task<CreateNotificationDto> MakeInAppNotification(int userId)
    {
      var (employee, user) = await GetEmployeeFromUserIdAsync(userId);

      CreateNotificationDto dto = new CreateNotificationDto
      {
        Subject = "Your new Role is ready",
        Message = $"You Role Has Been Updated from {user.Role} to {user.TempRole}",
        EmployeeId = employee.EmployeeId,
        Type = NotificationType.RoleUpdate,
        Severity = NotificationSeverity.Warning,
        DeliveryChannel = DeliveryChannel.InApp,
        DueDate = DateTime.Now
      };

      return dto;
    }

    private async Task<CreateNotificationDto> MakeEmailNotification(int userId, string otp)
    {
      var (employee, user) = await GetEmployeeFromUserIdAsync(userId);
      string message = $"You Role Has Been Updated from {user.Role} to {user.TempRole}.\nHere is your One-Time-Pin: {otp} (This pin expires after {_stepSeconds / 60} minutes)";
      CreateNotificationDto dto = new CreateNotificationDto
      {
        Subject = "Employee Role Change",
        Message = message,
        EmployeeId = employee.EmployeeId,
        Type = NotificationType.RoleUpdate,
        Severity = NotificationSeverity.Information,
        DeliveryChannel = DeliveryChannel.Email,
        DueDate = DateTime.Now
      };
      return dto;
    }

    private async Task<(EmployeeDto employeeDto, User user)> GetEmployeeFromUserIdAsync(int userId)
    {
      User? user = await _userRepo.GetUserByIdAsync(userId) ??
      throw new KeyNotFoundException($"User {userId} Not Found");

      EmployeeDto? employeeDto = await _employeeService.GetEmployeeByEmailAsync(user.Email) ??
      throw new KeyNotFoundException($"No Employee From User Type (ID: {user.UserId})");

      return (employeeDto, user);
    }

    /// <summary>
    /// This method confirms that role update made by <see
    /// cref="IUserService.UpdateUserRoleAsync(int, DTOs.User.UpdateUserRoleRequestDto)" 
    /// is carried out and role update is finalised throughout the system
    /// </summary>
    /// <param name="userId">User whom the role is being updated</param>
    public async Task ConfirmUserRoleUpdateAsync(int userId)
    {
      User? existing = await _userRepo.GetUserByIdAsync(userId);
      if (existing != null)
      {
        existing.Role = (UserRole)existing.TempRole!;
        _ = await _userRepo.UpdateUserAsync(userId, existing);
        await _userRepo.SaveChangesAsync();
      }
    }
  }
}