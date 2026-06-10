namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Interfaces.AccessControl;
  using HRConnect.Api.Data;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  using HRConnect.Api.DTOs.AccessControl;

  public class RolesRepository : IRolesRepository
  {
    private readonly ApplicationDBContext _context;

    public RolesRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    public async Task<Roles?> GetRoleByNameAsync(string roleName)
    {
      Roles? role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
      if (role == null)
        return null;
      return role;
    }
    public async Task<Roles?> GetRoleByEnumAsync(RoleName roleName)
    {
      Roles? role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
      if (role == null)
        return null;
      return role;
    }
    public async Task<Roles?> GetRoleByIdAsync(int roleId)
    {
      Roles? role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
      if (role == null)
        return null;
      return role;
    }

    public async Task<IEnumerable<RolesDto>> GetAllRolesAsync()
    {
      return await _context.Roles.Select(
        r => new RolesDto
        {
          RoleId = r.RoleId,
          Name = r.Name,
          Description = r.Description,
          ParentRoleId = r.ParentRoleId,
          ChildRoles = r.ChildRoles.Select(c => new ChildRolesDto
          {
            RoleId = c.RoleId,
            Name = c.Name,
            Description = c.Description,
            ParentRoleId = c.ParentRoleId
          }).ToList()
        }).ToListAsync();
    }

  }
}