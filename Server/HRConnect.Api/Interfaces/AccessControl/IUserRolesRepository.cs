namespace HRConnect.Api.Interfaces.AccessControl
{
  using HRConnect.Api.Models;

  public interface IUserRolesRepository
  {
    Task MigrateUserEnumRoles();
    Task<Roles> GetRoleByFromUserIdAsync(int userId);
  }
}