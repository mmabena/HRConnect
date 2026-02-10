namespace HRConnect.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.DTOs.Employee;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using HRConnect.Api.Utils;
    using System.Globalization;
    using HRConnect.Api.Mappers;
    using System.Data.Common;

    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepo;
        public EmployeeService(IEmployeeRepository employeeRepo)
        {
            _employeeRepo = employeeRepo;
        }

        public Task<List<Employee>> GetAllEmployeesAsync()
        {
            return _employeeRepo.GetAllEmployeesAsync();
        }

        public Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return _employeeRepo.GetEmployeeByIdAsync(id);
        }

        public async Task<Employee> CreateEmployeeAsync(CreateEmployeeRequestDto employeeRequestDto)
        {
            ValidateEmployee(employeeRequestDto);
            employeeRequestDto.EmployeeId = await GenerateUniqueEmpId(employeeRequestDto.Surname);
            var new_employee = employeeRequestDto.ToEmployeeFromCreateDTO();
            return await _employeeRepo.CreateEmployeeAsync(new_employee);

        }

        private async Task<string> GenerateUniqueEmpId(string lastName)
        {
            string prefix = lastName.Length >= 3
                ? lastName.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture)
                : lastName.ToUpper(CultureInfo.InvariantCulture).PadRight(3, 'X');
            int nextNum = 1;

            var existingIds = await _employeeRepo.GetAllEmployeeIdsWithPrefix(prefix);
            if (existingIds.Count > 0)
            {
                var maxNum = existingIds
                        .Select(id => int.Parse(id.AsSpan(3), CultureInfo.InvariantCulture))
                        .Max();

                nextNum = maxNum + 1;
            }

            return $"{prefix}{nextNum:D3}";
        }


        private static void ValidateEmployee(CreateEmployeeRequestDto employeeRequestDto)
        {
            var allowedGenders = new[] {"Male", "Female", "Other"};
            if (string.IsNullOrWhiteSpace(employeeRequestDto.Name))
                throw new ArgumentException("Employee name is required");

            if (employeeRequestDto.Name.Length > 50)
                throw new ArgumentException("Employee name must not exceed 50 characters");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.Surname))
                throw new ArgumentException("Employee surname is required");
            
            if (employeeRequestDto.Surname.Length > 100)
                throw new ArgumentException("Employee name must not exceed 100 characters");
            
            if (!string.IsNullOrWhiteSpace(employeeRequestDto.IdNumber) && employeeRequestDto.IdNumber.Length != 13)
                throw new ArgumentException("ID Number must be 13 digits long");

            if (string.IsNullOrWhiteSpace(employeeRequestDto.Gender))
                throw new ArgumentException("Employee gender is required");

            if (!allowedGenders.Contains(employeeRequestDto.Gender, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Gender must be either Male, Female or Other");
            
            if (string.IsNullOrWhiteSpace(employeeRequestDto.ContactNumber))
                throw new ArgumentException("Employee contact number is required");

            if (employeeRequestDto.ContactNumber.Length !=10)
                throw new ArgumentException("Contact number must be 10 digits long");

            

            
        }

    }
}