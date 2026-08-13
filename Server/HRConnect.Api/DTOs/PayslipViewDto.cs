namespace HRConnect.Api.DTOs
{
    public class PayslipViewDto
    {
        // Salary
        public decimal MonthlySalary { get; set; }

        // Deductions
        public decimal MedicalAidDeduction { get; set; }

        public decimal PensionDeduction { get; set; }

        public decimal UIFDeduction { get; set; }

        public decimal TaxDeduction { get; set; }

        public decimal TotalDeductions { get; set; }

        // Net salary after deductions
        public decimal NetSalary { get; set; }

      
        public decimal TotalCompanyContributions { get; set; }
    }
}