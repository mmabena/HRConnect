namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.DTOs;
  using HRConnect.Api.DTOs.User;
  using HRConnect.Api.Interfaces;
  using Microsoft.AspNetCore.Mvc;
  [Route("api/auth")]
  [ApiController]
  public class AuthController : ControllerBase
  {
    private readonly IAuthService _authService;

    // Static readonly arrays for repeated error messages
    private static readonly string[] EmailAndPasswordRequiredErrors =
        [
            "Email and password are required."
        ];

    private static readonly string[] InvalidDomainErrors =
        [
            "Email must be from singular.co.za domain."
        ];

    private static readonly string[] InvalidCredentialsErrors =
        [
            "Invalid email or password."
        ];

    public AuthController(IAuthService authService)
    {
      _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
      if (string.IsNullOrWhiteSpace(loginDto?.Email) || string.IsNullOrWhiteSpace(loginDto?.Password))
      {
        return BadRequest(new { errors = EmailAndPasswordRequiredErrors });
      }

      if (!loginDto.Email.EndsWith("@singular.co.za", StringComparison.OrdinalIgnoreCase))
      {
        return BadRequest(new { errors = InvalidDomainErrors });
      }

      string? token;
      try
      {
        token = await _authService.LoginAsync(loginDto.Email, loginDto.Password);
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { errors = new[] { ex.Message } });
      }

      if (string.IsNullOrWhiteSpace(token))
      {
        return Unauthorized(new { errors = InvalidCredentialsErrors });
      }

      var user = await _authService.GetUserByEmailAsync(loginDto.Email);
      if (user == null)
        return Unauthorized(new { errors = InvalidCredentialsErrors });

      return Ok(new
      {
        token,
        user = new
        {
          id = user.UserId,
          email = user.Email,
          role = user.Role.ToString()
        }
      });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
      try
      {
        var (pin, expiresAt) = await _authService.ForgotPasswordAsync(dto.Email);
        return Ok(new { message = "PIN sent to your email.", pin, expiresAt });
      }
      catch (ArgumentException ex)
      {
        ModelState.AddModelError("Email", ex.Message);
        return ValidationProblem(ModelState);
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(new { errors = new[] { ex.Message } }); // dynamic
      }
    }

    [HttpPost("verify-pin")]
    public async Task<IActionResult> VerifyPin([FromBody] VerifyPinDto dto)
    {
      try
      {
        await _authService.VerifyPinAsync(dto.Email, dto.Pin);
        return Ok(new { message = "PIN verified. You can now reset your password." });
      }
      catch (ArgumentException ex)
      {
        return BadRequest(new { errors = new[] { ex.Message } }); // dynamic
      }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
      try
      {
        await _authService.ResetPasswordAsync(dto);
        return Ok(new { message = "Password reset successfully." });
      }
      catch (ArgumentException ex)
      {
        ModelState.AddModelError("Validation", ex.Message);
        return ValidationProblem(ModelState);
      }
      catch (KeyNotFoundException ex)
      {
        return NotFound(new { errors = new[] { ex.Message } }); // dynamic
      }
    }
  }
}
