namespace HRConnect.Api.Interfaces.AccessControl
{
  using HRConnect.Api.Models;
  public interface IPermissionsRepository
  {
    Task<IEnumerable<Permissions>> GetAllPermissionsAsync();
    Task<Permissions?> GetPermissionByKeyAsync(string key);
    Task<IEnumerable<Permissions>> GetPermissionsForRoleAsync(int roleId);
    ///<summary>
    /// Method is crucial in mapping a permission or set of permissions to a role(s)
    /// Roles are assumed to exist in the system, if not they're are added at 
    /// start up and remain dynamic at runtime
    ///<paramref name="roleName">Name of the role to assign permissions to</paramref>
    ///<paramref name="permissionsList">Parameter collection of Permissions to be assigned</paramref>
    ///</summary>
    Task AssignPermissionsToRoleAsync(int roleId, params string[] permissionsList);
    //I'll Challenge myself of implementing this completely alone under pressure
    Task RemovePermissionsFromRoleAsync(int roleId, params string[] permissionsList);
    Task Save();
  }
}