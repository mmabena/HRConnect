namespace HRConnect.Api.DTOs.User
{
  using HRConnect.Api.Models;
  public class UpdateUserRequestDto
  {
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public UserRole Role { get; set; }
  }
}
