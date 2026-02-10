
namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;

  public class PayrollContributionCalculatorService : IPayrollDeductionService
  {
    private readonly ISeedEmployeeRepo _seedEmployeeRepo;
    private readonly IPayrollContributionsRepo _payrollContributionsRepo;
    public PayrollContributionCalculatorService(ISeedEmployeeRepo seedEmployeeRepo, IPayrollContributionsRepo payrollContributionsRepo)
    {
      _seedEmployeeRepo = seedEmployeeRepo;
      _payrollContributionsRepo = payrollContributionsRepo;
    }
    public async Task<Employee?> GetEmployeeByCodeAsync(string employeeCode)
    {
      return await _seedEmployeeRepo.GetEmployeeByCodeAsync(employeeCode);
    }
    public async Task<List<PayrollDeduction>> GetAllUifDeductions()
    {
      return await _payrollContributionsRepo.GetAllUifDeductionsAsync();
    }
    public async Task<Employee?> DeductUifAsync(int employeeId)
    {
      var employee = await _seedEmployeeRepo.GetEmployeeByIdAsync(employeeId);
      if (employee == null) return null;

      decimal employeeContribution = employee.MonthlySalary * UIFConstants.UIFEmployeeAmount;

      decimal employerContribution = employee.MonthlySalary * UIFConstants.UIFEmployeeAmount;
      //Ensure contribution cannot be more than R17 712.00
      if (employeeContribution >= UIFConstants.UIFCap)
      {
        employeeContribution = UIFConstants.UIFCap;
      }
      else if (employeeContribution == 0m)
      {
        employeeContribution = 0m;
      }

      employee.MonthlySalary -= employeeContribution;

      _ = await _payrollContributionsRepo.AddUifDeductionAsync(employeeContribution, employerContribution, 0m, employee.EmployeeId);
      return await _seedEmployeeRepo.UpdateEmployeeAsync(employeeId, employee);
    }

  }
}
