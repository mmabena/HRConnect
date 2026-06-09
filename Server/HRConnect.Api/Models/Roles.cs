namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  using System.Text.Json.Serialization;

  ///<summary>
  /// Self-Referencing table that holds all role data
  /// Table describes role hierarchy and inheritance-roles
  ///  CEO->HOD->SuperUser->NormalUser
  ///</summary>
  [Flags]
  public enum RoleName { NormalUser = 0, SuperUser, ExecutiveUser, CEO }

  public class Roles
  {
    [Key]
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public RoleName RoleName { get; set; }
    public string Description { get; set; } = string.Empty;
    //Self-referencing key 
    public int? ParentRoleId { get; set; }
    [JsonIgnore]
    public Roles? ParentRole { get; set; }
    public IList<Roles> ChildRoles { get; set; } = [];
    //Join table navigation properties
    public IList<RolePermissions> RolePermissions { get; set; } = [];
    public IList<UserRoles> UserRoles { get; set; } = [];
  }
}