namespace HRConnect.Api.Interfaces.AccessControl
{
  public interface IPermissionService
  {
    Task AssignPermissionsToRoleAsync(int roleId, params string[] permissionsList);
  }
}