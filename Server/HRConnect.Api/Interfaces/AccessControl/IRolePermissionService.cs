namespace HRConnect.Api.Interfaces.AccessControl
{
  using HRConnect.Api.Models;
  public interface IRolePermissionService
  {
    Task AssignPermissionsToRoleByIdAsync(int roleId, params string[] permissionsList);
    Task RemovePermissionsFromRoleAsync(int roleId, params int[] permissionsIds);
    Task<IEnumerable<Permissions>> GetPermissionsForRoleAsync(int roleId);
  }
}