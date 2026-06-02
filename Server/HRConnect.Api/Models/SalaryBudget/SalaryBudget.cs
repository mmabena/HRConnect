namespace HRConnect.Api.Models.SalaryBudget
{
  using System;

  public enum SalaryBudgetStatus
  {
    Draft,
    Submitted,
    Approved,
    Rejected,
    Archived
  }

  public class SalaryBudget
  {
    public int SalaryBudgetId { get; set; }
    public string SalaryBudgetName { get; set; } = string.Empty;
    public int BudgetYear { get; set; } 
    public SalaryBudgetStatus Status { get; set; }
    public string? RejectionReason { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedDate { get; set; }
    public DateTime? ArchivedDate { get; set; }
    public ICollection<SalaryBudgetEmployee> Employees { get; set; } = [];
  }
}