namespace HRConnect.Api.DTOs
{
    using System;
    using HRConnect.Api.Models;
    public class OnboardingMedicalEligibilityRequestDto
    {
        public decimal Salary { get; set; }

        public EmploymentStatus EmploymentStatus { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public string EmployeeSurname { get; set; } = string.Empty;

        public int NumberOfPrincipals { get; set; }

        public int NumberOfAdults { get; set; }

        public int NumberOfChildren { get; set; }

    }
}