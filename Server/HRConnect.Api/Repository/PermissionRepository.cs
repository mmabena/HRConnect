namespace HRConnect.Api.Repositories
{
  using HRConnect.Api.Interfaces.AccessControl;
  using HRConnect.Api.Data;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;

  public class PermissionRepository : IPermissionsRepository
  {
    private readonly ApplicationDBContext _context;
    private readonly IRolesRepository _rolesRepo;

    public PermissionRepository(ApplicationDBContext context, IRolesRepository rolesRepo)
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

    public async Task AssignPermissionsToRoleAsync(int roleId, params string[] permissionsList)
    {
      //Get the roles
      Roles? role = await _context.Roles
        .Include(r => r.RolePermissions)
        .FirstOrDefaultAsync(r => r.RoleId == roleId);
      //Get the permisions
      var permissions = await _context.Permissions
        .Where(p => permissionsList.Contains(p.Key)).ToListAsync();

      foreach (var p in permissions)
      {
        bool alreadyAssigned = role!.RolePermissions.Any(rp => rp.PermissionsId == p.PermissionsId);

        //Avoid duplicatte assignment
        if (alreadyAssigned)
          continue;

        await _context.RolePermissions.AddAsync(new RolePermissions
        {
          RoleId = role.RoleId,
          PermissionsId = p.PermissionsId,
          Role = role,
          Permissions = p,
          IsGranted = true
        });
      }

      //write to db
      await _context.SaveChangesAsync();
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
    public async Task Save()
    {
      await _context.SaveChangesAsync();
    }
  }
}