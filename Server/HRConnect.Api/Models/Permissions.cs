namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  /// <summary>
  /// Holds the permissions of a role from the hierarchy tree
  /// e.g employeee.view-own -> very important to prevent duplication
  /// </summary>
  public class Permissions
  {
    [Key]
    public int PermissionsId { get; }
    public string Key { get; set; } = string.Empty;
    // permissions duplication
    public string Description { get; set; } = string.Empty;
    // public string Category { get; set; } = string.Empty;
    public ICollection<RolePermissions> RolePermissions { get; set; } = [];
  }
}