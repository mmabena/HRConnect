namespace HRConnect.Api.Utils.ValidationHelpers.Deduction
{
  using System.ComponentModel.DataAnnotations;
  using HRConnect.Api.DTOs.Payroll.Deduction;
  using HRConnect.Api.Models.PayrollDeduction;

  public static class ValidateDeductionDto
  {
    public static void ValidateDeductionAddDto(DeductionAddDto deductionAddDto)
    {
      if (string.IsNullOrEmpty(deductionAddDto.CompanyId))
      {
        throw new ValidationException("Company ID is required");
      }

      if (string.IsNullOrEmpty(deductionAddDto.ShortDescription))
      {
        throw new ValidationException("Short description is required");
      }

      if (string.IsNullOrEmpty(deductionAddDto.LongDescription))
      {
        throw new ValidationException("Long description is required");
      }

      if (string.IsNullOrEmpty(deductionAddDto.DeductionType))
      {
        throw new ValidationException("Deduction type is required");
      }

      if (!Enum.IsDefined(deductionAddDto.InputType))
      {
        throw new ValidationException("Invalid InputType. Must be 'Amount' or 'Percentage'.");
      }

      if (deductionAddDto.MinimumValue is not null and < 0)
      {
        throw new ValidationException("Minimum value cannot be negative");
      }

      if (deductionAddDto.MaximumValue is not null and < 0)
      {
        throw new ValidationException("Maximum value cannot be negative");
      }

      if (deductionAddDto.MinimumValue is not null && deductionAddDto.MaximumValue is not null &&
        deductionAddDto.MinimumValue > deductionAddDto.MaximumValue)
      {
        throw new ValidationException("Minimum value cannot be greater than maximum value");
      }
    }

    public static void ValidateDeductionUpdateDto(DeductionUpdateDto deductionUpdateDto)
    {
      if (string.IsNullOrEmpty(deductionUpdateDto.DeductionId))
      {
        throw new ValidationException("Deduction code is required");
      }

      if (deductionUpdateDto.CompanyId is not null and "")
      {
        throw new ValidationException("Company ID cannot be empty");
      }

      if (deductionUpdateDto.ShortDescription is not null and "")
      {
        throw new ValidationException("Short description cannot be empty");
      }
      if (deductionUpdateDto.LongDescription is not null and "")
      {
        throw new ValidationException("Long description cannot be empty");
      }
      if (deductionUpdateDto.DeductionType is not null and "")
      {
        throw new ValidationException("Deduction type cannot be empty");
      }
      if (deductionUpdateDto.InputType.HasValue &&
        !Enum.IsDefined(typeof(DeductionInputType), deductionUpdateDto.InputType))
      {
        throw new ValidationException("Invalid InputType. Must be 'Amount' or 'Percentage'.");
      }
      if (deductionUpdateDto.MinimumValue is not null and < 0)
      {
        throw new ValidationException("Minimum value cannot be negative");
      }
      if (deductionUpdateDto.MaximumValue is not null and < 0)
      {
        throw new ValidationException("Maximum value cannot be negative");
      }
      if (deductionUpdateDto.MinimumValue is not null && deductionUpdateDto.MaximumValue is not null &&
        deductionUpdateDto.MinimumValue > deductionUpdateDto.MaximumValue)
      {
        throw new ValidationException("Minimum value cannot be greater than maximum value");
      }
    }
  }
}
