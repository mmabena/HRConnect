namespace HRConnect.Api.Utils.Pension.ValidationHelpers
{
  using System.ComponentModel.DataAnnotations;
  using HRConnect.Api.DTOs.Payroll.Pension;

  public static class ValidatePensionDeductionDtos
  {
    private static readonly decimal MAX_PENSIONCONTRIBUTION_PERCENTAGE = (decimal)27.5 / 100;
    public static void ValidateAddDto(PensionDeductionAddDto pensionDeductionAddDto)
    {
      if (string.IsNullOrWhiteSpace(pensionDeductionAddDto.EmployeeId))
      {
        throw new ValidationException("Employee ID is a required field and is not valid");
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
