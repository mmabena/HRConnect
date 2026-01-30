namespace HRConnect.Api.DTOs.User
{
  using HRConnect.Api.Models;
  public class CreateUserRequestDto
  {
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
  }
}