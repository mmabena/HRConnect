namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Models;
  using HRConnect.Api.Interfaces.AccessControl;
  using System.Linq;
  using Microsoft.EntityFrameworkCore;
  using System.Collections.Generic;

  public class RolePermissionRepository : IRolePermissionRepository
  {
    private readonly ApplicationDBContext _context;
    private readonly IRolesRepository _roleRepo;
    public RolePermissionRepository(ApplicationDBContext context,
     IRolesRepository roleRepository)
    {
      _context = context;
      _roleRepo = roleRepository;
    }
    public async Task AssignPermissionsToRoleByIdAsync(int roleId, params string[] permissionsList)
    {
      Roles? role = await _context.Roles
        .Include(r => r.RolePermissions)
        .FirstOrDefaultAsync(r => r.RoleId == roleId);

      List<Permissions> permissions = await _context.Permissions
        .Where(p => permissionsList.Contains(p.Key)).ToListAsync();

      foreach (Permissions p in permissions)
      {
        bool alreadyAssigned = role!.RolePermissions.Any(rp => rp.PermissionsId == p.PermissionsId);

        if (alreadyAssigned)
          continue;

        _ = await _context.RolePermissions.AddAsync(new RolePermissions
        {
          RoleId = role.RoleId,
          PermissionsId = p.PermissionsId,
          Role = role,
          Permissions = p,
          IsGranted = true
        });
      }

      await _context.SaveChangesAsync();
    }

    public async Task RemovePermissionsFromRoleAsync(int roleId, params int[] permissionsIds)
    {
      List<int> permissionIds = await _context.Permissions
      .Where(p => permissionsIds.Contains(p.PermissionsId))
      .Select(s => s.PermissionsId)
      .ToListAsync();

      List<RolePermissions> rolePermissions = await _context.RolePermissions
      .Where(rp => rp.RoleId == roleId &&
      permissionIds.Contains(rp.PermissionsId))
      .ToListAsync();

      _context.RolePermissions.RemoveRange(rolePermissions);
      await _context.SaveChangesAsync();
    }
    public async Task<IEnumerable<Permissions>> GetPermissionsForRoleAsync(int roleId)
    {
      Roles? role = await _roleRepo.GetRoleByIdAsync(roleId);
      if (role == null)
        throw new KeyNotFoundException($"This Role Does Not Exist");
      //Get all pairs of (roleId,<permission>)
      var rolePermissions = await _context.RolePermissions
      .Where(rp => rp.RoleId == role.RoleId)
      .Select(rp => rp.Permissions).ToListAsync();
      return rolePermissions;
    }
  }
}