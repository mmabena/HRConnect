namespace HRConnect.Api.Interfaces
{
  using Microsoft.EntityFrameworkCore;
  using HRConnect.Api.Interfaces.AccessControl;
  using HRConnect.Api.Models;
  using HRConnect.Api.Data;

  public class UserRolesRepository : IUserRolesRepository
  {

    private readonly ApplicationDBContext _context;
    public UserRolesRepository(ApplicationDBContext context)
    {
      _context = context;
    }
    public async Task MigrateUserEnumRoles()
    {
      //Get all users and sift through their roles 
      var users = await _context.Users.ToListAsync();
      //early return for when there are no users before RBAC
      if (users.Count <= 0)
        return;

      throw new NotImplementedException();
    }

  }
}