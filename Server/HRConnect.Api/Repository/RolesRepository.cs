namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Interfaces.AccessControl;
  using HRConnect.Api.Data;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;

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

    public async Task<Roles?> GetRoleByIdAsync(int roleId)
    {
      Roles? role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
      if (role == null)
        return null;
      return role;
    }

    public async Task<IEnumerable<Roles>> GetAllRolesAsync()
    {
      return await _context.Roles.ToListAsync();
    }

    public async Task AssignPermissionsToRoleByIdAsync(int roleId, params string[] permissionsList)
    {
      throw new NotImplementedException();
    }

    public async Task RemovePermissionsFromRoleAsync(int roleId, int permissionId)
    {
      throw new NotImplementedException();
    }
  }
}