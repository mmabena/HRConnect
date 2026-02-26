namespace HRConnect.Api.Mappers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs;
  using HRConnect.Api.DTOs.Employee;
  using HRConnect.Api.Models;

  public static class EmployeeMapper
  {
    public static EmployeeDto ToEmployeeDto(this Employee employeeModel)
    {
      return new EmployeeDto
      {
        EmployeeId = employeeModel.EmployeeId,
        Title = employeeModel.Title,
        Name = employeeModel.Name,
        Surname = employeeModel.Surname,
        IdNumber = employeeModel.IdNumber,
        PassportNumber = employeeModel.PassportNumber,
        Gender = employeeModel.Gender,
        ContactNumber = employeeModel.ContactNumber,
        TaxNumber = employeeModel.TaxNumber,
        Email = employeeModel.Email,
        PhysicalAddress = employeeModel.PhysicalAddress,
        City = employeeModel.City,
        ZipCode = employeeModel.ZipCode,
        HasDisability = employeeModel.HasDisability,
        DisabilityDescription = employeeModel.DisabilityDescription,
        DateOfBirth = employeeModel.DateOfBirth,
        StartDate = employeeModel.StartDate,
        Branch = employeeModel.Branch,
        MonthlySalary = employeeModel.MonthlySalary,
        PositionId = employeeModel.PositionId,
        EmploymentStatus = employeeModel.EmploymentStatus,
        CareerManagerID = employeeModel.CareerManagerID,
        ProfileImage = employeeModel.ProfileImage,
        CreatedAt = employeeModel.CreatedAt,
        UpdatedAt = employeeModel.UpdatedAt
      };
    }
    public static Employee ToEmployeeFromCreateDTO(this CreateEmployeeRequestDto employeeRequestDto)
    {
      return new Employee
      {
        EmployeeId = employeeRequestDto.EmployeeId,
        Title = employeeRequestDto.Title,
        Name = employeeRequestDto.Name,
        Surname = employeeRequestDto.Surname,
        IdNumber = employeeRequestDto.IdNumber,
        PassportNumber = employeeRequestDto.PassportNumber,
        Gender = employeeRequestDto.Gender!.Value,
        ContactNumber = employeeRequestDto.ContactNumber,
        TaxNumber = employeeRequestDto.TaxNumber,
        Email = employeeRequestDto.Email,
        PhysicalAddress = employeeRequestDto.PhysicalAddress,
        City = employeeRequestDto.City,
        ZipCode = employeeRequestDto.ZipCode,
        HasDisability = employeeRequestDto.HasDisability,
        DisabilityDescription = employeeRequestDto.DisabilityDescription,
        DateOfBirth = employeeRequestDto.DateOfBirth,
        StartDate = employeeRequestDto.StartDate,
        Branch = employeeRequestDto.Branch,
        MonthlySalary = employeeRequestDto.MonthlySalary,
        PositionId = employeeRequestDto.PositionId,
        EmploymentStatus = employeeRequestDto.EmploymentStatus,
        CareerManagerID = employeeRequestDto.CareerManagerID,
        ProfileImage = employeeRequestDto.ProfileImage
      };
    }
  }
}
