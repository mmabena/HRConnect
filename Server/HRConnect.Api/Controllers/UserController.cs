namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.DTOs.User;
  using HRConnect.Api.Mappers;
  using Microsoft.AspNetCore.Mvc;
  [Route("api/user")]
  [ApiController]
  public class UserController : ControllerBase
  {
    private readonly HRConnect.Api.Interfaces.IUserService _userService;

    public UserController(HRConnect.Api.Interfaces.IUserService userService)
    {
      _userService = userService;
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
  }
}