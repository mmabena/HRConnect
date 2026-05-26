
namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.DTOs.User;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Mappers;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  [Route("api/user")]
  [ApiController]
  public class UserController : ControllerBase
  {
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
      _userService = userService;
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
    {
      try
      {
        var result = await _userService.ChangePasswordAsync(dto);
        if (result)
          return Ok("Password changed successfully.");
        return BadRequest("Password change failed.");
      }
      catch (ArgumentException ex)
      {
        return BadRequest(ex.Message);
      }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
      var users = await _userService.GetAllUsersAsync();
      return Ok(users.Select(s => s.ToUserDto()));
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserById(int userId)
    {
      var user = await _userService.GetUserByIdAsync(userId);
      if (user == null) return NotFound();
      return Ok(user.ToUserDto());
    }

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetUserByEmailAsync(string email)
    {
      var user = await _userService.GetUserByEmailAsync(email);
      if (user == null) return NotFound();
      return Ok(user.ToUserDto());
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoleOptions()
    {
      var roles = await _userService.GetRoleOptionsAsync();
      return Ok(roles);
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser(int UserId, [FromBody] UpdateUserRequestDto updatedUser)
    {
      try
      {
        var result = await _userService.UpdateUserAsync(UserId, updatedUser);
        if (result == null) return NotFound();
        return NoContent();
      }
      catch (ArgumentException ex)
      {
        ModelState.AddModelError("Validation", ex.Message);
        return ValidationProblem(ModelState);
      }
    }

    [HttpPut("{userId}/role")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] UpdateUserRoleRequestDto request)
    {
      try
      {
        var result = await _userService.UpdateUserRoleAsync(userId, request);
        if (result == null)
          return NotFound();
        return Ok(result.ToUserDto());
      }
      catch (ArgumentException ex)
      {
        ModelState.AddModelError("Validation", ex.Message);
        return ValidationProblem(ModelState);
      }
    }

    [HttpPut("employee/{employeeId}/role")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> UpdateEmployeeUserRole(string employeeId, [FromBody] UpdateUserRoleRequestDto request)
    {
      try
      {
        var result = await _userService.UpdateEmployeeUserRoleAsync(employeeId, request);
        if (result == null)
          return NotFound();
        return Ok(result.ToUserDto());
      }
      catch (ArgumentException ex)
      {
        ModelState.AddModelError("Validation", ex.Message);
        return ValidationProblem(ModelState);
      }
    }
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(int UserId)
    {
      var deleted = await _userService.DeleteUserAsync(UserId);
      if (!deleted) return NotFound();
      return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto userDto)
    {
      try
      {
        var created = await _userService.CreateUserAsync(userDto);
        return CreatedAtAction(nameof(GetUserById), new { created.UserId }, created.ToUserDto());
      }
      catch (ArgumentException ex)
      {
        ModelState.AddModelError("Validation", ex.Message);
        return ValidationProblem(ModelState);
      }
    }
  }
}