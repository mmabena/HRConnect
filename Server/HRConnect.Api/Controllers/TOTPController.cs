namespace HRConnect.Api.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using HRConnect.Api.Interfaces.TOTP;
  using HRConnect.Api.DTOs.TOTP;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.RateLimiting;

  [EnableRateLimiting("totp-policy")]
  [Route("api/totp")]
  [ApiController]
  public class TOTPController : ControllerBase
  {
    private readonly ITOTPService _totpService;
    private readonly IMFAUserSecretsService _mfaService;
    public TOTPController(ITOTPService totpService, IMFAUserSecretsService mfaService)
    {
      _totpService = totpService;
      _mfaService = mfaService;
    }
    // [Authorize(Roles = "SuperUser")]
    [HttpPost("/sendOtp/{userId}")]
    public async Task<IActionResult> SendOTP(int userId)
    {
      await _totpService.SendTotpAndNotify(userId);
      return Ok();
    }
    [HttpPost("/verify")]
    public async Task<IActionResult> VerifyPin([FromBody] TOTPValidateRequestDto dto)
    {
      //get the user secret
      byte[] storedSecret = await _mfaService.GetOrCreateUserSecretAsync(dto.UserId);
      //match it agains code
      if (await _totpService.ValidateCodeAsync(dto.UserId, storedSecret, dto.Code))
      {
        await _totpService.ConfirmUserRoleUpdateAsync(dto.UserId);
        return Ok($"Success. Your Role Access Will Take Affect On Next Login");
      }
      else
      {
        return BadRequest($"Failed To Validate Your OTP");
      }
    }

    // public Task<IActionResult> ResendNewPin()
    // {
    //   throw new NotImplementedException();
    // }
  }
}
