
namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils;

  public class PayrollContributionCalculatorService : IPayrollDeductionService
  {
    private readonly ISeedEmployeeRepo _seedEmployeeRepo;
    private readonly PayrollContributionCalculator _deductionsCalculator;
    private readonly IPayrollContributionsRepo _payrollContributionsRepo;
    public PayrollContributionCalculatorService(ISeedEmployeeRepo seedEmployeeRepo, IPayrollContributionsRepo payrollContributionsRepo)
    {
      _seedEmployeeRepo = seedEmployeeRepo;
      _payrollContributionsRepo = payrollContributionsRepo;
      _deductionsCalculator = new PayrollContributionCalculator();
    }
    /// <summary>
    /// Getting a Seeded Employee
    /// </summary>
    /// <param name="employeeCode"></param>
    /// <returns></returns>
    public async Task<Employee?> GetEmployeeByCodeAsync(string employeeCode)
    {
      return await _seedEmployeeRepo.GetEmployeeByCodeAsync(employeeCode);
    }
    /// <summary>
    /// Getting deductions by Seeded Employee id
    /// </summary>
    /// <param name="employeeId"></param>
    /// <returns></returns>
    public async Task<PayrollDeduction?> GetDeductionsByEmployeeIdAsync(int employeeId)
    {
      return await _payrollContributionsRepo.GetDeductionsByEmployeeIdAsync(employeeId);
    }

    public async Task<List<PayrollDeduction>> GetAllDeductionsAsync()
    {
      return await _payrollContributionsRepo.GetAllDeductionsAsync();
    }
    public async Task<PayrollDeduction?> AddDeductionsAsync(int employeeId)
    {
      Employee? employee = await _seedEmployeeRepo.GetEmployeeByIdAsync(employeeId);
      if (employee == null) return null;

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
        SdlAmount = sdlDeduction
      };

      employee.MonthlySalary -= employeeAmount;

      _ = await _seedEmployeeRepo.UpdateEmployeeAsync(employeeId, employee);
      return await _payrollContributionsRepo.AddDeductionsAsync(deductions);
    }
  }
}
