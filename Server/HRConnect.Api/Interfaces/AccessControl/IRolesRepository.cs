namespace HRConnect.Api.Interfaces.AccessControl
{
  using HRConnect.Api.DTOs.AccessControl;
  using HRConnect.Api.Models;
  public interface IRolesRepository
  {
    Task<Roles?> GetRoleByNameAsync(string roleName);
    Task<IEnumerable<RolesDto>> GetAllRolesAsync();
    Task<RolesDto?> GetRoleByIdAsync(int roleId);
    // Task RemovePermissionsFromRoleAsync(int roleId, int permissionId);
    // Task AssignPermissionsToRoleByIdAsync(int roleId, params string[] permissionsList);
  }
}