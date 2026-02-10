namespace HRConnect.Api.Mappers
{
  using HRConnect.Api.DTOs.PayrollDeductions;
  using HRConnect.Api.Models;

  public static class PayrollDeductionMapper
  {
    public static PayrollDeductionsDto ToPayrollDeductionsDto(this PayrollDeduction payrollDeductionModel)
    {
      return new PayrollDeductionsDto
      {
        SdlAmount = payrollDeductionModel.SdlAmount,
        UifEmployeeAmount = payrollDeductionModel.UifEmployeeAmount,
        UifEmployerAmount = payrollDeductionModel.UifEmployerAmount
      };
    }
  }
}