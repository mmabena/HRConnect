namespace HRConnect.Api.Interfaces.AccessControl
{
  using HRConnect.Api.Models;
  public interface IPermissionService
  {
    Task<IEnumerable<Permissions>> GetAllPermissionsAsync();
    Task<Permissions?> GetPermissionByIdAsync(int id);
    Task<Permissions?> GetPermissionByKeyAsync(string key);
    Task<IEnumerable<Permissions>> GetPermissionsForRoleAsync(int roleId);
  }
}