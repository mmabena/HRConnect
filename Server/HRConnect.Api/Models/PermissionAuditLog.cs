namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  public class PermissionAuditLog
  {
    [Key]
    public int Id { get; }
    public int RoleId { get; set; }
    public Roles Role { get; set; } = null!;
    public int PermissionId { get; set; }
    public Permissions Permissions { get; set; } = null!;
    public string Action { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
  }
}