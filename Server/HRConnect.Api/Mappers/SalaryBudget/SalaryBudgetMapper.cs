namespace HRConnect.Api.Mappers.SalaryBudget
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.Models;
  using HRConnect.Api.Dtos.SalaryBudget;
  public class SalaryBudgetMapper
  {

    public static SalaryBudgetDto ToSalaryBudgetDto(this SalaryBudget salaryBudgetModel)
    {
      return new SalaryBudgetDto
      {
        SalaryBudgetId = salaryBudgetModel.SalaryBudgetId,
        SalaryBudgetName = salaryBudgetModel.SalaryBudgetName,
        BudgetYear = salaryBudgetModel.BudgetYear,
        SalaryBudgetStatus = salaryBudgetModel.SalaryBudgetStatus,
        RejectionReason = salaryBudgetModel.RejectionReason,
        CreatedDate = salaryBudgetModel.CreatedDate,
        ApprovedDate = salaryBudgetModel.ApprovedDate,
        ArchivedDate = salaryBudgetModel.ArchivedDate
      };
    }


    public static SalaryBudget ToSalaryBudgetCreateDto(this CreateSalaryBudgetDto createBudgetEmployeeDto)
    {
      return new SalaryBudgetDto
      {
        SalaryBudgetName = salaryBudgetModel.SalaryBudgetName,
        BudgetYear = salaryBudgetModel.BudgetYear,
        SalaryBudgetStatus = salaryBudgetModel.SalaryBudgetStatus,
        CreatedDate = salaryBudgetModel.CreatedDate
      };
    }

  }
}
