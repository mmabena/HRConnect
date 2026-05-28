namespace HRConnect.Api.Repositories
{
  using HRConnect.Api.Interfaces.AccessControl;
  using HRConnect.Api.Data;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;

  public class PermissionsRepository : IPermissionsRepository
  {
    private readonly ApplicationDBContext _context;
    private readonly IRolesRepository _rolesRepo;

    public PermissionsRepository(ApplicationDBContext context, IRolesRepository rolesRepo)
    {
      _context = context;
      _rolesRepo = rolesRepo;
    }

    public async Task<Permissions?> GetPermissionByKeyAsync(string key)
    {
      Permissions? permission = await _context.Permissions.FirstAsync(p => p.Key == key);
      if (permission == null)
        return null;
      return permission;
    }

    public async Task AssignPermissionsToRoleAsync(int roleId, string[] permissionsList)
    {
      throw new NotImplementedException();
    }
    public async Task RemovePermissionsFromRoleAsync(int roleId, params string[] permissionsList)
    {
      throw new NotImplementedException();
    }

    public async Task<IEnumerable<Permissions>> GetPermissionsForRoleAsync(int roleId)
    {
      //Because Permissions and Roles have a joining table, I can just ask
      //it to give me all permissions referencing the given key
      return await _context.RolePermissions.Where(rp => rp.RoleId == roleId)
         .Select(rp => rp.Permissions)
         .ToListAsync();

    }

    public async Task<IEnumerable<Permissions>> GetAllPermissionsAsync()
    {
      return await _context.Permissions.ToListAsync();
    }
  }
}

