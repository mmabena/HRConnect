
namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils;
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
    /// <summary>
    /// Getting a Seeded Employee
    /// </summary>
    /// <param name="employeeCode"></param>
    /// <returns></returns>
    public async Task<Employee?> GetEmployeeByCodeAsync(string employeeCode)
    {
      return await _employeeRepo.GetEmployeeByCodeAsync(employeeCode);
    }
    /// <summary>
    /// Getting deductions by Seeded Employee id
    /// </summary>
    /// <param name="employeeId"></param>
    /// <returns></returns>
    public async Task<PayrollDeduction?> GetDeductionsByEmployeeIdAsync(int employeeId)
    {
      return await _payrollDeductionRepo.GetDeductionsByEmployeeIdAsync(employeeId);
    }
    public async Task<List<PayrollDeduction>> GetAllDeductionsAsync()
    {
      return await _payrollDeductionRepo.GetAllDeductionsAsync();
    }
    public async Task<PayrollDeduction?> AddDeductionsAsync(int employeeId)
    {
      Employee? employee = await _employeeRepo.GetEmployeeByIdAsync(employeeId);
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

      _ = await _employeeRepo.UpdateEmployeeAsync(employeeId, employee);
      return await _payrollDeductionRepo.AddDeductionsAsync(deductions);
    }
  }
}
