namespace HRConnect.Api.Services
{

  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.AccessControl;
  using HRConnect.Api.Interfaces.AccessControl;
  using HRConnect.Api.Mappers.AccessControl;

  public class RoleService : IRoleService
  {
    private readonly IRolesRepository _roleRepo;
    public RoleService(IRolesRepository roleRepository)
    {
      _roleRepo = roleRepository;
    }

    public async Task<RolesDto?> GetRoleByNameAsync(string roleName)
    {
      Roles? role = await _roleRepo.GetRoleByNameAsync(roleName);
      if (role == null)
        throw new KeyNotFoundException($"Role Named {roleName} Not Found");
      return role.ToRolesDtoFromRole();
    }
    public async Task<IEnumerable<RolesDto>> GetAllRolesAsync()
    {
      var roles = await _roleRepo.GetAllRolesAsync();
      return roles;
    }
    public async Task<RolesDto?> GetRoleByIdAsync(int roleId)
    {
      Roles? role = await _roleRepo.GetRoleByIdAsync(roleId);
      if (role == null)
        throw new KeyNotFoundException($"Role With ID {roleId} Not Found");
      return role.ToRolesDtoFromRole();
    }
  }
}