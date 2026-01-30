namespace HRConnect.Api.Models
{
  public enum UserRole
  {
    NormalUser,
    SuperUser
  }

  public class User
  {
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  }
}