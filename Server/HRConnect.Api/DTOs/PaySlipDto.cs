namespace HRConnect.Api.DTOs
{
  public class PayslipDto
  {
    // Employee Information
    public string EmployeeId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Surname { get; set; } = string.Empty;

    public string Position { get; set; } = string.Empty;

    public string EmploymentStatus { get; set; } = string.Empty;

    public string TaxNumber { get; set; } = string.Empty;

    public string IdNumber { get; set; } = string.Empty;

    public string ContactNumber { get; set; } = string.Empty;

    public string PhysicalAddress { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public int PositionId { get; set; }

    // Salary Information
    public decimal MonthlySalary { get; set; }

    public decimal NetSalary { get; set; }


    // Deductions
    public decimal MedicalAidDeduction { get; set; }

    public decimal PensionDeduction { get; set; }

    public decimal UIFDeduction { get; set; }

    public decimal TaxDeduction { get; set; }

    public decimal TotalCompanyContributions { get; set; }
  }
}