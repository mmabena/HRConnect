namespace HRConnect.Api.Utils.Pension.ValidationHelpers
{
  using System.ComponentModel.DataAnnotations;
  using HRConnect.Api.DTOs.Employee.Pension;

  public static class ValidateEmployeePensionEnrollmentDtos
  {
    private static readonly decimal MAX_PENSIONCONTRIBUTION_PERCENTAGE = (decimal)27.5 / 100;
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

      if (employeePensionEnrollmentAddDto.VoluntaryContribution is not null and < 0)
      {
        throw new ValidationException("Voluntary contribution cannot be less lower than zero");
      }

      if (employeePensionEnrollmentAddDto.VoluntaryContribution != null && employeePensionEnrollmentAddDto.VoluntaryContribution > decimal.Zero
        && employeePensionEnrollmentAddDto.IsVoluntaryContributionPermament is null)
      {
        throw new ValidationException("Voluntary contribution has to be permament or once-off");
      }

      DateOnly today = DateOnly.FromDateTime(DateTime.Today);

      if (employeePensionEnrollmentAddDto.EffectiveDate < today)
      {
        throw new ValidationException("Effective date must be today or in the future");
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
      //DateOnly nextMonth = today.AddMonths(1);
      DateOnly firstDayNextMonth = new DateOnly(today.Year, today.Month, 1).AddMonths(1);
      if (today.Day > 25 && employeePensionEnrollmentUpdateDto.EffectiveDate != null &&
        employeePensionEnrollmentUpdateDto.EffectiveDate != firstDayNextMonth)
      {
        throw new ValidationException("Effective date must be beginning of next month");
      }

      if (employeePensionEnrollmentUpdateDto.EffectiveDate < today)
      {
        throw new ValidationException("Effective date must be today or in the future");
      }

      if (employeePensionEnrollmentUpdateDto.VoluntaryContribution is not null and < decimal.Zero)
      {
        throw new ValidationException("Voluntary contribution cannot be less lower than zero");
      }

      if (employeePensionEnrollmentUpdateDto.VoluntaryContribution != null && employeePensionEnrollmentUpdateDto.VoluntaryContribution > decimal.Zero
        && employeePensionEnrollmentUpdateDto.IsVoluntaryContributionPermament is null)
      {
        throw new ValidationException("Voluntary contribution has to be permament or once-off");
      }

      /*if (employeePensionEnrollmentUpdateDto.PayrollRunId is not null and (not < 1 or > 12))
      {
        throw new ValidationException("Payroll run ID is invalid");
      }*/
    }
    public static void ValidateVoluntaryContribution(decimal voluntaryContribution, decimal employeeMonthSalary, decimal pensionOptionPercentage)
    {
      float voluntaryContributionPercentage = (float)Math.Round(voluntaryContribution / employeeMonthSalary, 2);

      if ((voluntaryContributionPercentage + ((float)pensionOptionPercentage / 100)) > (float)MAX_PENSIONCONTRIBUTION_PERCENTAGE)
      {
        throw new ValidationException("Voluntary Contribution + Monthly Salary Contribution cannot exceed 27.5% of salary");
      }
    }
  }
}
