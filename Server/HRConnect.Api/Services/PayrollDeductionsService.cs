
namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils;
  using HRConnect.Api.Mappers;
  using Microsoft.AspNetCore.Mvc;
  using HRConnect.Api.DTOs.PayrollDeduction;
  using Audit.Core;

  public class PayrollDeductionsService : IPayrollDeductionsService
  {
    private readonly IEmployeeRepository _employeeRepo;
    private readonly PayrollDeductionsCalculator _deductionsCalculator;
    private readonly IPayrollDeductionsRepository _payrollDeductionRepo;
    public PayrollDeductionsService(IEmployeeRepository employeeRepo, IPayrollDeductionsRepository payrollContributionsRepo)
    {
      _employeeRepo = employeeRepo;
      _payrollDeductionRepo = payrollContributionsRepo;
      _deductionsCalculator = new PayrollDeductionsCalculator();
    }

    public async Task<PayrollDeductionDto?> GetDeductionsByEmployeeIdAsync(string employeeId)
    {
      var deduction = await _payrollDeductionRepo.GetDeductionsByEmployeeIdAsync(employeeId);
      if (deduction == null)
        return null;
      return deduction.ToPayrollDeductionDto();
    }
    public async Task<IEnumerable<PayrollDeductionDto>> GetAllDeductionsAsync()
    {
      var deductions = await _payrollDeductionRepo.GetAllDeductionsAsync();
      return deductions.Select(d => d.ToPayrollDeductionDto()).ToList();
    }
    /// <summary>
    /// This method adds and entry for payroll deductions. It further tracks and audits deductions using a scoped approach
    /// </summary>
    /// <param name="employeeId">Id of employee whose salary deductions are calculated</param>
    /// <returns>Newly added deductions</returns>
    /// <exception cref="KeyNotFoundException">Thrown when employeeId does not return an existing employee</exception>
    /// <exception cref="ArgumentException">Generic exception should this method fail with an existing employee</exception>
    public async Task<PayrollDeduction?> AddDeductionsAsync(string employeeId)
    {

      using (var scope = AuditScope.Create("PayrollDeduction:Insert",
            () => new { employeeId }, EventCreationPolicy.InsertOnEnd))
      {
        try
        {
          Employee? employee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);
          if (employee == null) throw new KeyNotFoundException();

          var (employeeAmount, employerAmount) = _deductionsCalculator.CalculateUif(employee.MonthlySalary);
          var sdlDeduction = _deductionsCalculator.CalculateSdlAmount(employee.MonthlySalary);
          var deductions = new PayrollDeduction
          {
            EmployeeId = employee.EmployeeId,
            MonthlySalary = employee.MonthlySalary,
            PassportNumber = employee.PassportNumber,
            IdNumber = employee.IdNumber,
            UifEmployeeAmount = employeeAmount,
            UifEmployerAmount = employerAmount,
            EmployerSdlContribution = sdlDeduction
          };

          var newDeduction = await _payrollDeductionRepo.AddDeductionsAsync(deductions);
          decimal projectedSalary = newDeduction.MonthlySalary - newDeduction.UifEmployeeAmount;

          scope.Event.Environment = null;

          return newDeduction;
        }

        catch (ArgumentException ex)
        {
          //Discards audit capturing should no deduction take place
          scope.Discard();
          throw new ArgumentException($"Failed To Add deductions {ex.Message}");
        }
      }
    }
  }
}