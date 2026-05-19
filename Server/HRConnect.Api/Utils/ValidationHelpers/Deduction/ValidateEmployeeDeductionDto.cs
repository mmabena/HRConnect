namespace HRConnect.Api.Utils.ValidationHelpers.Deduction
{
  using System.ComponentModel.DataAnnotations;
  using HRConnect.Api.DTOs.Payroll.Deduction;

  public static class ValidateEmployeeDeductionDto
  {
    public static void ValidateEmployeeDeductionAddDto(EmployeeDeductionAddDto employeeDeductionAddDto)
    {
      if (employeeDeductionAddDto.EmployeeId == "")
      {
        throw new ValidationException("Employee Id must be a valid value.");
      }

      if (employeeDeductionAddDto.DeductionId == "" || employeeDeductionAddDto.DeductionId.Length < 6)
      {
        throw new ValidationException("Deduction code must be valid value ");
      }

      if (employeeDeductionAddDto.AmountOrPercentage <= 0)
      {
        throw new ValidationException("Amount or Percentage must be valid");
      }
    }

    public static void ValidateEmployeeDeductionUpdateDto(EmployeeDeductionUpdateDto employeeDeductionUpdateDto)
    {
      if (employeeDeductionUpdateDto.EmployeeId == "")
      {
        throw new ValidationException("Employee Id must be a valid value.");
      }

      if (employeeDeductionUpdateDto.DeductionId == "" || employeeDeductionUpdateDto.DeductionId.Length < 6)
      {
        throw new ValidationException("Deduction code must be valid value ");
      }

      if (employeeDeductionUpdateDto.AmountOrPercentage <= 0)
      {
        throw new ValidationException("Amount or Percentage must be valid");
      }
    }
  }
}
