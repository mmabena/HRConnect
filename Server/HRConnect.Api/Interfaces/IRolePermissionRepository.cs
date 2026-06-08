namespace HRConnect.Api.Interfaces
{
  public interface IRolePermissionRepository
  {
    /// <summary>
    /// Assigns Permissions to Roles
    /// </summary>
    /// <param name="roleId">Id of the Role which the permission is being assigned to</param>
    /// <param name="permissionsList">Permission in Scope Notation. E.g
    /// <resource>.<action> </param>
    Task AssignPermissionsToRoleByIdAsync(int roleId, params string[] permissionsList);

    Task RemovePermissionsFromRoleAsync(int roleId, params string[] permissionsList);
  }
}