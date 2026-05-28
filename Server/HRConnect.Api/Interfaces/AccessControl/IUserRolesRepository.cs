namespace HRConnect.Api.Interfaces.AccessControl
{
  public interface IUserRolesRepository
  {
    Task MigrateUserEnumRoles();
  }
}