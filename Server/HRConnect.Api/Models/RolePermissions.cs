namespace HRConnect.Api.Models
{
  // A joining table. IDK Why I need this yet
  public class RolePermissions
  {
    public int RoleId { get; set; }
    public int PermissionsId { get; set; }
    public bool IsGranted { get; set; }
  }
}