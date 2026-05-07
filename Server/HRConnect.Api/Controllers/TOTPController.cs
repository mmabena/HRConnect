namespace HRConnect.Api.Controllers
{
  using Microsoft.AspNetCore.Mvc;
  using HRConnect.Api.DTOs.TOTP;

  [Route("api/totp")]
  [ApiController]
  public class TOTPController : ControllerBase
  {

    [HttpPost("/verify")]
    public Task<IActionResult> VerifyPin([FromBody] TOTPValidateRequestDto dto)
    {
      throw new NotImplementedException();
    }
    public Task<IActionResult> ResendNewPin()
    {
      throw new NotImplementedException();
    }
  }
}