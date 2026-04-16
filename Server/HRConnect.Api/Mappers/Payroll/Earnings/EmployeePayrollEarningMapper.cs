namespace HRConnect.Api.Mappers.Payroll.Earnings
{
  using HRConnect.Api.DTOs.Payroll.Earnings;
  using HRConnect.Api.Models.Payroll.Earnings;

  public static class EmployeePayrollEarningMapper
  {
    public static EmployeePayrollEarning ToEmployeePayrollEarningModel(this EmployeePayrollEarningAddDto employeePayrollEarningAddDto)
    {
      return new EmployeePayrollEarning
      {
        EmployeeId = employeePayrollEarningAddDto.EmployeeId,
        PayrollEarningId = employeePayrollEarningAddDto.PayrollEarningId,
        OverTimeHoursWorked = employeePayrollEarningAddDto.OverTimeHoursWorked,
        Amount = employeePayrollEarningAddDto.Amount ?? decimal.Zero
      };
    }
    public static EmployeePayrollEarningDto ToEmployeePayrollEarningDto(this EmployeePayrollEarning employeePayrollEarning)
    {
      return new EmployeePayrollEarningDto
      {
        EmployeePayrollEarningId = employeePayrollEarning.EmployeePayrollEarningId,
        EmployeeId = employeePayrollEarning.EmployeeId,
        PayrollEarningId = employeePayrollEarning.PayrollEarningId,
        TaxCode = employeePayrollEarning.TaxCode,
        Amount = employeePayrollEarning.Amount,
        PayrollRunId = employeePayrollEarning.PayrollRunId,
        IsLocked = employeePayrollEarning.IsLocked
      };
    }
  }
}
