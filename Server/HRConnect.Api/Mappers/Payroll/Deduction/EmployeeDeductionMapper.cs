namespace HRConnect.Api.Mappers.Payroll.Deduction
{
  using HRConnect.Api.DTOs.Payroll.Deduction;
  using HRConnect.Api.Models.PayrollDeduction;

  public static class EmployeeDeductionMapper
  {
    public static EmployeeDeductionDto ToEmployeeDeductionDto(this EmployeeDeduction employeeDeduction)
    {
      return new EmployeeDeductionDto
      {
        EmployeeDeductionId = employeeDeduction.EmployeeDeductionId,
        EmployeeId = employeeDeduction.EmployeeId,
        DeductionId = employeeDeduction.DeductionId,
        DeductionType = employeeDeduction.DeductionType,
        DeductionInputType = employeeDeduction.DeductionInputType,
        AmountOrPercentage = employeeDeduction.AmountOrPercentage,
        CalculatedDeductionAmount = employeeDeduction.CalculatedDeductionAmount,
        PayRunId = employeeDeduction.PayrollRunId,
        IsLocked = employeeDeduction.IsLocked,
      };
    }

    public static EmployeeDeduction ToEmployeeDeductionModel(this EmployeeDeductionAddDto employeeDeductionAddDto)
    {
      return new EmployeeDeduction
      {
        EmployeeId = employeeDeductionAddDto.EmployeeId,
        DeductionId = employeeDeductionAddDto.DeductionId,
        DeductionType = "",
        AmountOrPercentage = employeeDeductionAddDto.AmountOrPercentage
      };
    }
  }
}
