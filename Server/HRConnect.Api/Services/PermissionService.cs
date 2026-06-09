namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces.AccessControl;
  using HRConnect.Api.Models;
  public class PermissionService : IPermissionService
  {
    private readonly IPermissionsRepository _permissionRepo;
    public PermissionService(IPermissionsRepository permissionRepository)
    {
      _permissionRepo = permissionRepository;
    }
    public async Task<IEnumerable<Permissions>> GetAllPermissionsAsync()
    {
      var permissions = await _permissionRepo.GetAllPermissionsAsync();
      return permissions;
    }
    public async Task<Permissions?> GetPermissionByIdAsync(int id)
    {
      var permission = await _permissionRepo.GetPermissionsByIdAsync(id);
      if (permission == null)
        throw new KeyNotFoundException($"Permission Was Not Found With Id ${id}");

      return permission;
    }

    public async Task<Permissions?> GetPermissionByKeyAsync(string key)
    {
      var permission = await _permissionRepo.GetPermissionByKeyAsync(key);
      if (permission == null)
        throw new KeyNotFoundException($"Permission Was Not Found With Id ${key}");

      return permission;
    }

    public async Task<IEnumerable<Permissions>> GetPermissionsForRoleAsync(int roleId)
    {
      throw new NotImplementedException();
    }

  }
}