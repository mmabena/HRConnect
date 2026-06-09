namespace HRConnect.Api.Interfaces.AccessControl
{
  using HRConnect.Api.DTOs.AccessControl;
  public interface IRoleService
  {
    Task<RolesDto?> GetRoleByNameAsync(string roleName);
    Task<IEnumerable<RolesDto>> GetAllRolesAsync();
    Task<RolesDto?> GetRoleByIdAsync(int roleId);
  }
}