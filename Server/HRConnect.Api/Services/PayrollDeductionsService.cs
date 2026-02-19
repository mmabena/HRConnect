<<<<<<< HEAD

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

    public async Task<PayrollDeduction?> GetDeductionsByEmployeeIdAsync(string employeeId)
    {
      return await _payrollDeductionRepo.GetDeductionsByEmployeeIdAsync(employeeId);
    }
    public async Task<List<PayrollDeduction>> GetAllDeductionsAsync()
    {
      return await _payrollDeductionRepo.GetAllDeductionsAsync();
    }
    public async Task<PayrollDeduction?> AddDeductionsAsync(string employeeId)
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
=======

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

    public async Task<PayrollDeduction?> GetDeductionsByEmployeeIdAsync(string employeeId)
    {
      return await _payrollDeductionRepo.GetDeductionsByEmployeeIdAsync(employeeId);
    }
    public async Task<List<PayrollDeductionsDto>> GetAllDeductionsAsync()
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

        employee.MonthlySalary -= employeeAmount;

        _ = await _employeeRepo.UpdateEmployeeAsync(employeeId, employee);
        return await _payrollDeductionRepo.AddDeductionsAsync(deductions);
      }
      catch (ArgumentException ex)
      {
        throw new ArgumentException($"Failed To Add deductions {ex.Message}");
      }
    }
  }
}
>>>>>>> 6f925a0edeaed929a59e86c64f891a0419502b7b
