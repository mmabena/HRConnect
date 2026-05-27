namespace HRConnect.Api.Models
{
  using System.ComponentModel.DataAnnotations;
  public class RoleUpdateAuditLog
  {
    [Key]
    public int Id { get; }
    public int UserId { get; set; }
    public string PreviousRole { get; set; } = string.Empty;
    public string NewRole { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
  }
}