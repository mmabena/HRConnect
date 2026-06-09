namespace HRConnect.Api.DTOs.AccessControl
{
  public class AssignPermissionToRoleDTO
  {
    // public int RoleId { get; set; }
    public string[] PermissionsArray { get; set; } = [];
  }
}