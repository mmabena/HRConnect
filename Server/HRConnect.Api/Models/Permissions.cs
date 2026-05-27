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
    public int PermissionsId { get; set; }//IsUnique
    public string PermissionsKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    //Category?
    public RoleName RoleName { get; set; }
  }
}
//Recommended additions
//IsSystemRole to prevent accidental deletion of core permissions 