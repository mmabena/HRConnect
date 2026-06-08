namespace HRConnect.Api.Models
{
  public enum Userrole
  {
    NormalUser,
    SuperUser,
    ExecutiveUser,
    CEO
  }

  public class User
  {
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    // TODO 
    public Userrole Role { get; set; }
    public Userrole? TempRole { get; set; }
    public ICollection<UserRoles> UserRoles { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  }
}