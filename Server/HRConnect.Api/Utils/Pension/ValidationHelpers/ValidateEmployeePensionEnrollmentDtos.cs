namespace HRConnect.Api.Utils.Pension.ValidationHelpers
{
  using System.ComponentModel.DataAnnotations;
  using HRConnect.Api.DTOs.Employee.Pension;

  public static class ValidateEmployeePensionEnrollmentDtos
  {
    public static void ValidateAddDto(EmployeePensionEnrollmentAddDto employeePensionEnrollmentAddDto)
    {
      /*if (employeePensionEnrollmentAddDto.PensionOptionId <= 0)
      {
        throw new ValidationException("Pension option ID is a required field and is not valid");
      }*/

      if (string.IsNullOrWhiteSpace(employeePensionEnrollmentAddDto.EmployeeId))
      {
        throw new ValidationException("Employee ID is a required field and is not valid");
      }

      if (employeePensionEnrollmentAddDto.EffectiveDate is null)
      {
        throw new ValidationException("Effective date must be beginning of next month");
      }
    }

    public static void ValidateUpdateDto(EmployeePensionEnrollmentUpdateDto employeePensionEnrollmentUpdateDto)
    {
      if (string.IsNullOrWhiteSpace(employeePensionEnrollmentUpdateDto.EmployeeId))
      {
        throw new ValidationException("Employee ID is a required field and is not valid");
      }

      if (employeePensionEnrollmentUpdateDto.PensionOptionId <= 0)
      {
        throw new ValidationException("Pension option ID is not valid");
      }

      DateOnly today = DateOnly.FromDateTime(DateTime.Now);
      DateOnly nextMonth = today.AddMonths(1);
      if (employeePensionEnrollmentUpdateDto.EffectiveDate is not null &&
        ((employeePensionEnrollmentUpdateDto.EffectiveDate.Value.Month != nextMonth.Month) ||
        (employeePensionEnrollmentUpdateDto.EffectiveDate.Value.Year != nextMonth.Year)))
      {
        throw new ValidationException("Effective date must be beginning of next month");
      }

      if (employeePensionEnrollmentUpdateDto.PayrollRunId is not null and (not < 1 or > 12))
      {
        throw new ValidationException("Payroll run ID is invalid");
      }
    }
  }
}
