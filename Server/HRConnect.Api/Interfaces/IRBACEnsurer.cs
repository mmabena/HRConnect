namespace HRConnect.Api.Interfaces
{

  public interface IRBACEnsurer
  {
    Task EnsureRoleBasedAccessControlAsync();
  }
}