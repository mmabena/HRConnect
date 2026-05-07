namespace HRConnect.Api.Services
{
  using OtpNet;
  using Microsoft.Extensions.Configuration;
  using HRConnect.Api.Interfaces.TOTP;
  using HRConnect.Api.Models;
  using HRConnect.Api.Interfaces;
  using Microsoft.EntityFrameworkCore.Design;

  public class TOTPService : ITOTPService
  {
    private readonly ITOTPRepository _totpRepo;
    private readonly int _stepSeconds;
    private readonly IUserService _userService;
    private readonly IMFAUserSecretsService _mfaService;
    public TOTPService(ITOTPRepository totpRepo, IUserService userService,
    IMFAUserSecretsService mfaService, IConfiguration configuration)
    {
      _totpRepo = totpRepo;

      //Use configured step minutes or fall back to 10 minutes
      _stepSeconds = ResolveStepDuration(configuration);
      _userService = userService; ;
      _mfaService = mfaService;
    }

    public async Task SendTotp(int userId)
    {
      try
      {
        //First make sure the user exists
        User? user = await _userService.GetUserByIdAsync(userId);
        if (user == null)
          throw new KeyNotFoundException();

        byte[] secret = await _mfaService.GetOrCreateUserSecretAsync(user.UserId);
        string code = await GenerateCodeAsync(secret);

        Console.WriteLine($"/////////////////////THIS IS THE OTP {code}");
      }
      catch (OperationException ex)
      {
        throw new OperationException($"Failed To Send OTP {ex.Message}");
      }
    }
    public async Task<string> GenerateCodeAsync(byte[] userSecret)
    {
      Totp otpCode = new(userSecret, step: _stepSeconds, OtpHashMode.Sha256);
      Console.WriteLine($">>>>>>>>>[[[[[THE TIME-BASED ONE TIME PIN IS {otpCode}]]]]]]<<<<<<");

      Console.WriteLine($">>>>>>>>>[[[[[THE TOPT Computed IS {otpCode.ComputeTotp()}]]]]]]<<<<<<");

      ///Create notifications to send the notifications
      return otpCode.ComputeTotp();
    }
    public async Task<bool> ValidateCodeAsync(int userId, byte[] userSecret, string code)
    {
      Totp otpCode = new(userSecret, step: _stepSeconds, OtpHashMode.Sha256);
      //TOTP are generated every 10 minutes (size of out step),
      // VerificationWindow.prev=1 step back (10 minutes back) 
      // VerificationWindow.futu=1 step forward (10 minutes ahead) 
      // step size == Step(Minutes/Seconds)
      bool isValid = otpCode.VerifyTotp(
        code,
        out long timeStepMatched,
        new VerificationWindow(previous: 1, future: 1));

      if (!isValid)
        return false;

      //Check for replays 
      if (await IsReplayAsync(userId, timeStepMatched))
        return false;

      // mark this code as being used so that it cannot be reused within verification 
      // window
      await MarkUsedCodeAsync(userId, timeStepMatched);
      return true;
    }
    //This is our way of trying to prevent against replay and ensure keys are used only once 
    public async Task<bool> IsReplayAsync(int userId, long stepCount)
    {
      return await _totpRepo.IsReplay(userId, stepCount);
    }
    public async Task MarkUsedCodeAsync(int userId, long stepCount)
    {
      await _totpRepo.MarkUsedAsync(userId, stepCount);
    }
    private int ResolveStepDuration(IConfiguration configuration)
    {
      int minutes = configuration.GetValue("Totp:StepMinutes", 10);
      if (minutes <= 0) minutes = 10;

      int seconds = Math.Max(minutes, 1) * 60;
      if (seconds <= 0) return seconds * 600;

      return seconds;
    }
  }
}