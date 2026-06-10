namespace HRConnect.Api.Services
{
  using System.Data.Common;
  using HRConnect.Api.Interfaces.AccessControl;
  using HRConnect.Api.Models;

  public class RolePermissionService : IRolePermissionService
  {
    private readonly IRolePermissionRepository _rolePermissionRepo;
    public RolePermissionService(IRolePermissionRepository rolePermissionRepo)
    {
      _rolePermissionRepo = rolePermissionRepo;
    }

    public async Task AssignPermissionsToRoleByIdAsync(int roleId, params string[] permissionsList)
    {
      try
      {
        if (roleId <= 0)
          throw new ArgumentException("RoleID must be valid and greater than 0");
        await _rolePermissionRepo.AssignPermissionsToRoleByIdAsync(roleId, permissionsList);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed To Assign Permission To Role; {ex.Message}");
      }
    }
    public async Task RemovePermissionsFromRoleAsync(int roleId, params int[] permissionsIds)
    {
      try
      {
        await _rolePermissionRepo.RemovePermissionsFromRoleAsync(roleId, permissionsIds);
      }
      catch (DbException ex)
      {
        throw new InvalidOperationException($"Failed To Delete Permissions From Role: {ex.Message}");
      }
    }

    public async Task<IEnumerable<Permissions>> GetPermissionsForRoleAsync(int roleId)
    {
      try
      {
        if (roleId <= 0)
          throw new ArgumentException("RoleID must be valid and greater than 0");
        return await _rolePermissionRepo.GetPermissionsForRoleAsync(roleId);
      }
      catch (Exception ex)
      {
        throw new InvalidOperationException($"Failed To Fetch Permissions To Role; {ex.Message}");
      }
    }
  }
}