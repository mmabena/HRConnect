namespace HRConnect.Api.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    using HRConnect.Api.DTOs.Employee;
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
                Email = employeeModel.Email,
                PhysicalAddress = employeeModel.PhysicalAddress,
                DateOfBirth = employeeModel.DateOfBirth,
                StartDate = employeeModel.StartDate,
                Branch = employeeModel.Branch,
                MonthlySalary = employeeModel.MonthlySalary,
                PositionId = employeeModel.PositionId,
                CareerManager = employeeModel.CareerManager,
                EmpPicture = employeeModel.EmpPicture,
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
                Gender = employeeRequestDto.Gender,
                ContactNumber = employeeRequestDto.ContactNumber,
                Email = employeeRequestDto.Email,
                PhysicalAddress = employeeRequestDto.PhysicalAddress,
                DateOfBirth = employeeRequestDto.DateOfBirth,
                StartDate = employeeRequestDto.StartDate,
                Branch = employeeRequestDto.Branch,
                MonthlySalary = employeeRequestDto.MonthlySalary,
                PositionId = employeeRequestDto.PositionId,
                EmploymentStatus = employeeRequestDto.EmploymentStatus,
                CareerManager = employeeRequestDto.CareerManager,
                EmpPicture = employeeRequestDto.EmpPicture
            };
        }
    }
}