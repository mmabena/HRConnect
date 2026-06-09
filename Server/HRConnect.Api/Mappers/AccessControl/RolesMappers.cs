namespace HRConnect.Api.Mappers.AccessControl
{
  using HRConnect.Api.DTOs.AccessControl;
  using HRConnect.Api.Models;

  public static class RolesMappers
  {
    public static RolesDto ToRolesDtoFromRole(this Roles role)
    {
      return new RolesDto
      {
        RoleId = role.RoleId,
        Name = role.Name,
        Description = role.Description,
        ParentRoleId = role.ParentRoleId,
        // ChildRoles = role.ChildRoles,
        // RolePermissions = role.RolePermissions
      };
    }
    public static Roles ToRoleFromRolesDto(this RolesDto dto)
    {
      return new Roles
      {

      };
    }
  }
}