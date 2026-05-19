using HRConnect.Api.DTOs.User;
using HRConnect.Api.Mappers;
using Microsoft.AspNetCore.Mvc;
using HRConnect.Api.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace HRConnect.Api.Controllers
{
  [Route("api/user")]
  [ApiController]
  public class UserController : ControllerBase
  {
    private readonly HRConnect.Api.Interfaces.IUserService _userService;


    public UserController(HRConnect.Api.Interfaces.IUserService userService)
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

    [HttpGet("{UserId}")]
    public async Task<IActionResult> GetUserById(int UserId)
    {
      var user = await _userService.GetUserByIdAsync(UserId);
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

    [HttpPut("{UserId}")]
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

    [HttpPut("{UserId}/role")]
    [Authorize(Roles = "SuperUser")]
    public async Task<IActionResult> UpdateUserRole(int UserId, [FromBody] UpdateUserRoleRequestDto request)
    {
      try
      {
        var result = await _userService.UpdateUserRoleAsync(UserId, request);
        if (result == null) return NotFound();
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
        if (result == null) return NotFound();
        return Ok(result.ToUserDto());
      }
      catch (ArgumentException ex)
      {
        ModelState.AddModelError("Validation", ex.Message);
        return ValidationProblem(ModelState);
      }
    }

    [HttpDelete("{UserId}")]
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

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
      var email =
        User.FindFirst(ClaimTypes.Email)?.Value ??
        User.FindFirst("email")?.Value;

      if (string.IsNullOrEmpty(email))
        return Unauthorized();

      var result = await _userService.GetCurrentUserAsync(email);

      if (result == null)
        return NotFound();

      return Ok(result);

    }
  }
}
