namespace HRConnect.Api.Interfaces.Payroll.Deduction
{
  using HRConnect.Api.DTOs.Payroll.Deduction;

  public interface IEmployeeDeductionService
  {
    Task<EmployeeDeductionDto> AddAsync(EmployeeDeductionAddDto employeeDeductionAddDto);
    Task<List<EmployeeDeductionDto>> GetAllAsync();
    Task<List<EmployeeDeductionDto>> GetByEmployeeIdAsync(string employeeId);
    Task<List<EmployeeDeductionDto>> GetByEmployeeIdAndIsNotLockedAsync(string employeeId);
    Task<List<EmployeeDeductionDto>> GetByEmployeeIdAndLastRunIdAsync(string employeeId);
    Task<List<EmployeeDeductionDto>> GetByPayrollRunIdAsync(int payrollRunId);
    Task<List<EmployeeDeductionDto>> GetByDeductionIdAsync(string deductionId);
    Task<EmployeeDeductionDto> UpdateAsync(EmployeeDeductionUpdateDto employeeDeductionUpdateDto);
    Task LockEmployeeDeductionsAsync();
    Task RollOverEmployeePayrollEarningsAsync();
  }
}
