namespace HRConnect.Api.DTOs.Employee
{
    using System;
    using HRConnect.Api.Models;

    public abstract class EmployeeBaseRequestDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public Title Title { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public string? Nationality { get; set; }
        public Gender? Gender { get; set; }
        public string ContactNumber { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhysicalAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public bool HasDisability { get; set; }
        public string? DisabilityDescription { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public DateOnly StartDate { get; set; }
        public Branch Branch { get; set; }
        public decimal MonthlySalary { get; set; }
        public int PositionId { get; set; }
        public EmploymentStatus EmploymentStatus { get; set; }
        public string? CareerManagerID { get; set; } = string.Empty;
        public string ProfileImage { get; set; } = string.Empty;
    }
}
