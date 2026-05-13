namespace HRConnect.Api.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using HRConnect.Api.Interfaces.TOTP;
  using Microsoft.AspNetCore.Authorization;

  [Route("api/totp")]
  [ApiController]
  public class TOTPController : ControllerBase
  {
    private readonly ITOTPService _totpService;
    public TOTPController(ITOTPService totpService)
    {
      _totpService = totpService;
    }
    [Authorize(Roles = "SuperUser")]
    [HttpPost("/sendOtp/{userId}")]
    public async Task<IActionResult> SendOTP(int userId)
    {
      await _totpService.SendTotp(userId);
      return Ok();
    }
    // [HttpPost("/verify")]
    // public Task<IActionResult> VerifyPin([FromBody] TOTPValidateRequestDto dto)
    // {
    //   throw new NotImplementedException();
    // }
    // public Task<IActionResult> ResendNewPin()
    // {
    //   throw new NotImplementedException();
    // }
  }
}