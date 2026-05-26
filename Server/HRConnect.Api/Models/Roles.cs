namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  ///<summary>
  ///Self-Referencing table
  ///</summary>
  [Flags]
  public enum RoleName { NormalUser = 0, SuperUser, ExecutiveUser, CEO }
  public class Roles
  {
    [Key]
    public int RoleId { get; set; }
    //This might need to be an Enum
    public string Name { get; set; } = string.Empty;
    public RoleName _Name { get; set; }
    public int ParenteRoleId { get; set; }//Roles follow a linear hierarchy 
    public IList<Permissions> Permissions { get; set; } = [];
  }
}