namespace HRConnect.Api.Interfaces
{
  using Microsoft.EntityFrameworkCore;
  using HRConnect.Api.Interfaces.AccessControl;
  using HRConnect.Api.Models;
  using HRConnect.Api.Data;

  public class UserRolesRepository : IUserRolesRepository
  {

    private readonly ApplicationDBContext _context;
    private readonly IRolesRepository _roleRepo;
    public UserRolesRepository(ApplicationDBContext context,
    IRolesRepository rolesRepository)
    {
      _context = context;
      _roleRepo = rolesRepository;
    }
    public async Task MigrateUserEnumRoles()
    {
      //Get all users and sift through their roles 
      var users = await _context.Users.ToListAsync();
      //early return for when there are no users before RBAC
      if (users.Count <= 0)
        return;

      foreach (User u in users)
      {
        Userrole legacyRole = u.Role;
        Roles? newRole = await _roleRepo
        .GetRoleByEnumAsync(LegacyRoleToNewRole(legacyRole));
        if (newRole == null)
          continue;

        bool exists = await _context.UserRoles.AnyAsync(ur =>
        ur.RolesId == newRole.RoleId &&
        ur.UserId == u.UserId);

        if (!exists)
        {
          _ = _context.UserRoles.Add(new UserRoles
          {
            Role = newRole,
            RolesId = newRole.RoleId,
            User = u,
            UserId = u.UserId
          });
        }
      }
      _ = await _context.SaveChangesAsync();
    }
    private static RoleName LegacyRoleToNewRole(Userrole legacyRole)
    {
      return legacyRole switch
      {
        Userrole.NormalUser => RoleName.NormalUser,
        Userrole.SuperUser => RoleName.SuperUser,
        _ => RoleName.NormalUser
      };
    }

    public async Task<Roles> GetRoleByFromUserIdAsync(int userId)
    {
      Roles? role = await _context.UserRoles.Where(ur => ur.UserId == userId)
      .Select(r => r.Role).FirstOrDefaultAsync();
      return role!;
    }
  }
}