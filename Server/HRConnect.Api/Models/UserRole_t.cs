namespace HRConnect.Api.Models
{
  public class UserRoles
  {
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int RolesId { get; set; }
    public Roles Role { get; set; } = null!;
  }
}