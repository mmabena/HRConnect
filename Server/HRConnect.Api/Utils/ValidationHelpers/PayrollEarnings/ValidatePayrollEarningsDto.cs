namespace HRConnect.Api.Utils.ValidationHelpers.PayrollEarnings
{
  using System.ComponentModel.DataAnnotations;
  using HRConnect.Api.DTOs.Payroll.Earnings;

  public static class ValidatePayrollEarningsDto
  {
    public static void ValidatePayrollEarningAddDto(PayrollEarningAddDto payrollEarningAddDto)
    {
      if (string.IsNullOrEmpty(payrollEarningAddDto.ShortDescription))
      {
        throw new ValidationException("Short description is required");
      }

      if (string.IsNullOrEmpty(payrollEarningAddDto.LongDescription))
      {
        throw new ValidationException("Long description is required");
      }

      if (payrollEarningAddDto.TaxCode is < 1000 or > 9999)
      {
        throw new ValidationException("Tax code must be between 1000 and 9999");
      }

      if (payrollEarningAddDto.Taxable && payrollEarningAddDto.TaxPercentage is null)
      {
        throw new ValidationException("Tax percentage is required when taxable is true");
      }

      if (payrollEarningAddDto.TaxPercentage is not null and
        (< decimal.Zero or > 100))
      {
        throw new ValidationException("Tax percentage must be between 0 and 100");
      }

      if (payrollEarningAddDto.OvertimeHourMultiplier is not null and
        (< decimal.Zero or decimal.Zero))
      {
        throw new ValidationException("Overtime hour multiplier cannot be lower than zero or equal to zero");
      }
    }

    public static void ValidatePayrollEarningUpdateDto(PayrollEarningUpdateDto payrollEarningUpdateDto)
    {
      if (string.IsNullOrEmpty(payrollEarningUpdateDto.PayrollEarningId))
      {
        throw new ValidationException("Payroll earning id is required");
      }

      if (payrollEarningUpdateDto.ShortDescription is not null and "")
      {
        throw new ValidationException("Short description cannot be empty");
      }

      if (payrollEarningUpdateDto.LongDescription is not null and "")
      {
        throw new ValidationException("Long description cannot be empty");
      }

      if (payrollEarningUpdateDto.TaxCode is not null and (< 1000 or > 9999))
      {
        throw new ValidationException("Tax code must be between 1000 and 9999");
      }

      if (payrollEarningUpdateDto.Taxable is not null && (payrollEarningUpdateDto.TaxPercentage == null))
      {
        throw new ValidationException("Tax percentage is required when taxable is true");
      }

      if (payrollEarningUpdateDto.TaxPercentage is not null and
        (< decimal.Zero or > 100))
      {
        throw new ValidationException("Tax percentage must be between 0 and 100");
      }

      if (payrollEarningUpdateDto.OvertimeHourMultiplier is not null and < decimal.Zero and not decimal.Zero)
      {
        throw new ValidationException("Overtime hour multiplier cannot be lower than zero or equal zero");
      }
    }
  }
}
