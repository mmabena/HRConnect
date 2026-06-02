namespace HRConnect.Api.DTOs.SalaryBudget
{
  using System;
  using HRConnect.Api.Models.SalaryBudget;

  public class SalaryBudgetDto
  {
    public int SalaryBudgetId { get; set; }
    public string SalaryBudgetName { get; set; } = string.Empty;
    public int BudgetYear { get; set; }
    public SalaryBudgetStatus Status { get; set; }
    public string? RejectionReason { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedDate { get; set; }
  }
}