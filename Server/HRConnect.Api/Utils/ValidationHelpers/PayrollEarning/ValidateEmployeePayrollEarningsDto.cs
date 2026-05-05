namespace HRConnect.Api.Utils.ValidationHelpers.PayrollEarning
{
  using System.ComponentModel.DataAnnotations;
  using HRConnect.Api.DTOs.Payroll.Earning;

  public static class ValidateEmployeePayrollEarningsDto
  {
    public static void ValidateEmployeePayrollEarningAddDto(EmployeePayrollEarningAddDto employeePayrollEarningAddDto)
    {
      if (string.IsNullOrEmpty(employeePayrollEarningAddDto.EmployeeId))
      {
        throw new ValidationException("Employee id is required");
      }

      if (string.IsNullOrEmpty(employeePayrollEarningAddDto.PayrollEarningId))
      {
        throw new ValidationException("Payroll earning id is required");
      }

      if (employeePayrollEarningAddDto.OverTimeHoursWorked is not null and <= 0)
      {
        throw new ValidationException("Overtime hours worked cannot be lower than zero");
      }

      if (employeePayrollEarningAddDto.Amount is not null and (< decimal.Zero or decimal.Zero))
      {
        throw new ValidationException("Amount cannot be lower than zero or equal zero");
      }

      if (employeePayrollEarningAddDto.OverTimeHoursWorked is not null && employeePayrollEarningAddDto.Amount is not null)
      {
        throw new ValidationException("Overtime hours worked and amount cannot be both provided as overtime is calculated systematically");
      }
    }

    public static void ValidateEmployeePayrollEarningUpdateDto(EmployeePayrollEarningUpdateDto employeePayrollEarningUpdateDto)
    {
      if (employeePayrollEarningUpdateDto.OverTimeHoursWorked is not null and <= 0)
      {
        throw new ValidationException("Overtime hours worked cannot be lower than zero or equal zero");
      }

      if (employeePayrollEarningUpdateDto.Amount is not null and (< decimal.Zero or decimal.Zero))
      {
        throw new ValidationException("Amount cannot be lower than zero or equal zero");
      }

      if (employeePayrollEarningUpdateDto.OverTimeHoursWorked is not null && employeePayrollEarningUpdateDto.Amount is not null)
      {
        throw new ValidationException("Overtime hours worked and amount cannot be both provided as overtime is calculated systematically");
      }
    }
  }
}
