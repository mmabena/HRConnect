namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Models;
  using HRConnect.Api.Interfaces;
  using System.Linq;
  using Microsoft.EntityFrameworkCore;
  using System.Collections.Generic;

  public class RolePermissionRepository : IRolePermissionRepository
  {
    private readonly ApplicationDBContext _context;
    public RolePermissionRepository(ApplicationDBContext context)
    {
      _context = context;
    }
    public async Task AssignPermissionsToRoleByIdAsync(int roleId, params string[] permissionsList)
    {
      //Get the roles
      Roles? role = await _context.Roles
        .Include(r => r.RolePermissions)
        .FirstOrDefaultAsync(r => r.RoleId == roleId);
      //Get the permisions
      List<Permissions> permissions = await _context.Permissions
        .Where(p => permissionsList.Contains(p.Key)).ToListAsync();

      foreach (Permissions p in permissions)
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

    public Task RemovePermissionsFromRoleAsync(int roleId, params string[] permissionsList)
    {
      throw new NotImplementedException();
    }
  }
}