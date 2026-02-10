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
        UifEmployerAmount = payrollDeductionModel.UifEmployerAmount,
        EmployeeId = payrollDeductionModel.EmployeeId,
        IdNumber = payrollDeductionModel.IdNumber,
        PassportNumber = payrollDeductionModel.PassportNumber,
        MonthlySalary = payrollDeductionModel.MonthlySalary
      };
    }
    public static PayrollDeduction ToPayrollDeductionsFromAddDeductionsDto(this PayrollDeductionsDto payrollDeductionsDto)
    {
      return new PayrollDeduction
      {
        UifEmployeeAmount = payrollDeductionsDto.UifEmployeeAmount,
        SdlAmount = payrollDeductionsDto.SdlAmount,
        UifEmployerAmount = payrollDeductionsDto.UifEmployerAmount,
        EmployeeId = payrollDeductionsDto.EmployeeId,
        IdNumber = payrollDeductionsDto.IdNumber,
        PassportNumber = payrollDeductionsDto.PassportNumber,
        MonthlySalary = payrollDeductionsDto.MonthlySalary
      };
    }
  }
}