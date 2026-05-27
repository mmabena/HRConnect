namespace HRConnect.Api.Models
{
  public enum UserRole
  {
    NormalUser,
    SuperUser,
    HOD,
    CEO
  }

  public class User
  {
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    // TODO 
    // MOVE THIS TO A LIST FOR A 1-MANY
    public UserRole Role { get; set; }
    public UserRole? TempRole { get; set; }
    public ICollection<UserRoles> UserRoles { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  }
}