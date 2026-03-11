namespace HRConnect.Api.Utils.Pension.ValidationHelpers
{
  using System.ComponentModel.DataAnnotations;
  using HRConnect.Api.DTOs.Payroll.Pension;

  public static class ValidatePensionDeductionDtos
  {
    public static void ValidateAddDto(PensionDeductionAddDto pensionDeductionAddDto)
    {
      if (string.IsNullOrWhiteSpace(pensionDeductionAddDto.EmployeeId))
      {
        throw new ValidationException("Employee ID is a required field and is not valid");
      }

      if (pensionDeductionAddDto.VoluntaryContribution is not null and < 0)
      {
        throw new ValidationException("Voluntary contribution is not valid");
      }
    }

    public static void ValidateUpdateDto(PensionDeductionUpdateDto pensionDeductionUpdateDto)
    {
      if (string.IsNullOrWhiteSpace(pensionDeductionUpdateDto.EmployeeId))
      {
        throw new ValidationException("Employee ID is a required field and is not valid");
      }

      if (pensionDeductionUpdateDto.PensionOptionId <= 0)
      {
        throw new ValidationException("Pension option ID is not valid");
      }

      if (pensionDeductionUpdateDto.VoluntaryContribution is not null and < 0)
      {
        throw new ValidationException("Voluntary contribution is not valid");
      }

      DateOnly today = DateOnly.FromDateTime(DateTime.Now);
      DateOnly nextMonth = today.AddMonths(1);
      if (pensionDeductionUpdateDto.CreatedDate is not null &&
        ((pensionDeductionUpdateDto.CreatedDate.Value.Month != nextMonth.Month) ||
        (pensionDeductionUpdateDto.CreatedDate.Value.Year != nextMonth.Year)))
      {
        throw new ValidationException("Effective date must be beginning of next month");
      }

      if (pensionDeductionUpdateDto.PayrollRunId is not null and (not < 1 or > 12))
      {
        throw new ValidationException("Payroll run ID is invalid");
      }
    }
  }
}
