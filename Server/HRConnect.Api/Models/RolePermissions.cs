namespace HRConnect.Api.Models
{
  // A joining table. IDK Why I need this yet
  /// <summary>
  /// Maps a permissions to a role
  /// </summary>
  public class RolePermissions
  {
    //(RoleId,PermissionsId)->Composite Keys
    public int RoleId { get; set; }
    public Roles Role { get; set; } = null!;
    public int PermissionsId { get; set; }
    public Permissions Permissions { get; set; } = null!;
    public bool IsGranted { get; set; }
  }
}
