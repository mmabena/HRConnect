namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  public class Permissions
  {
    [Key]
    public int PermissionsId { get; set; }
    public string PermissionsKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    //Category?
    public RoleName RoleName { get; set; }
  }
}