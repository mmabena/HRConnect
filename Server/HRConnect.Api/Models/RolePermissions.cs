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
    public int PermissionsId { get; set; }
    public bool IsGranted { get; set; }
  }
}
