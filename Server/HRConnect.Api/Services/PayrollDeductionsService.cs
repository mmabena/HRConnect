namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.DTOs.PayrollDeductions;

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
      return deduction.ToPayrollDeductionsDto();
    }
    public async Task<IEnumerable<PayrollDeductionDto>> GetAllDeductionsAsync()
    {
      var deductions = await _payrollDeductionRepo.GetAllDeductionsAsync();
      return deductions.Select(d => d.ToPayrollDeductionsDto()).ToList();
    }
    public async Task<PayrollDeduction?> AddDeductionsAsync(string employeeId)
    {
      try
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
        // employee.MonthlySalary -= employeeAmount;
        // await _employeeRepo.UpdateEmployeeAsync(employeeId, employee);
        return await _payrollDeductionRepo.AddDeductionsAsync(deductions);
      }
      catch (ArgumentException ex)
      {
        throw new ArgumentException($"Failed To Add deductions {ex.Message}");
      }
    }
  }
}
