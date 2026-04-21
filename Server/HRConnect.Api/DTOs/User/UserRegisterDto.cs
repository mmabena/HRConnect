namespace HRConnect.Api.DTOs.User
{
  public class UserRegisterDto
  {
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string Password { get; set; } = string.Empty;
  }
}