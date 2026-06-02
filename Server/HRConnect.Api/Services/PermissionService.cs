namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces.AccessControl;

  public class PermissionService : IPermissionService
  {

    public async Task AssignPermissionsToRoleAsync(int roleId, params string[] permissionsList)
    {
    }
  }
}