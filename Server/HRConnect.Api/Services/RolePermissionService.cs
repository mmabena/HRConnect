namespace HRConnect.Api.Services
{
  using System.Data.Common;
  using HRConnect.Api.Interfaces.AccessControl;

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
    public async Task RemovePermissionsFromRoleAsync(int roleId, params string[] permissionsList)
    {
      try
      {
        await _rolePermissionRepo.RemovePermissionsFromRoleAsync(roleId, permissionsList);
      }
      catch (DbException ex)
      {
        throw new InvalidOperationException($"Failed To Delete Permissions From Role: {ex.Message}");
      }
    }
  }
}