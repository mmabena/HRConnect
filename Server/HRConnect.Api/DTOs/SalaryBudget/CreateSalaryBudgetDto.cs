namespace HRConnect.Api.DTOs.SalaryBudget
{
  using System;
  using HRConnect.Api.Models.SalaryBudget;
  public class CreateSalaryBudgetDto
  {
    public string SalaryBudgetName { get; set; } = string.Empty;
    public int BudgetYear { get; set; }
    public SalaryBudgetStatus Status { get; set; }
  }
}