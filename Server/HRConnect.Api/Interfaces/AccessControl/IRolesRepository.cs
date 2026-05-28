namespace HRConnect.Api.Interfaces.AccessControl
{
  using HRConnect.Api.Models;

  public interface IRolesRepository
  {
    Task<Roles?> GetRoleByNameAsync(string roleName);
    Task<IEnumerable<Roles>> GetAllRolesAsync();

    // Task RemovePermissionsFromRoleAsync(int roleId, int permissionId);
    // Task AssignPermissionsToRoleAsync(int roleId, params string[] permissionsList);
  }
}