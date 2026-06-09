namespace HRConnect.Api.DTOs.AccessControl
{
  using HRConnect.Api.Models;
  public class RolesDto
  {
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ParentRoleId { get; set; }
    public IList<ChildRolesDto> ChildRoles { get; set; } = [];
    public IList<RolePermissions> RolePermissions { get; set; } = [];
    // public IList<UserRoles> UserRoles { get; set; } = [];
  }

  public class ChildRolesDto
  {
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ParentRoleId { get; set; }
  }
}