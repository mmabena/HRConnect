namespace HRConnect.Api.Interfaces.AccessControl
{
  using System.Threading.Tasks;

  public interface IRolePermissionService
  {
    Task AssignPermissionsToRoleByIdAsync(int roleId, params string[] permissionsList);
    Task RemovePermissionsFromRoleAsync(int roleId, params string[] permissionsList);
  }
}