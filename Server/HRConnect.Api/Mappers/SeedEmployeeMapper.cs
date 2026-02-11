namespace HRConnect.Api.Mappers
{
  using HRConnect.Api.DTOs.SeedEmployee;
  using HRConnect.Api.Models;

  public static class SeedEmployeeMapper
  {
    public static SeedEmployeeDto ToEmployeeDto(this Employee employeeModel)
    {
      return new SeedEmployeeDto
      {
        Name = employeeModel.Name,
        EmployeeCode = employeeModel.EmployeeCode,
        MonthlySalary = employeeModel.MonthlySalary,
        IdNumber = employeeModel.IdNumber,
        PassportNumber = employeeModel.PassportNumber
      };
    }
  }
}