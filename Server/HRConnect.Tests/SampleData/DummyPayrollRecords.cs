namespace HRConnect.Tests.SampleData;

using HRConnect.Api.Models.Payroll;
using HRConnect.Api.Models.PayrollDeduction;
using HRConnect.Api.DTOs.Employee;

/// <summary>
/// Sample payroll record generators for testing purposes
/// </summary>
public static class DummyPayrollRecords
{
    /// <summary>
    /// Creates a realistic dummy MedicalAidDeduction record
    /// </summary>
    /// <param name="id">Optional custom ID</param>
    /// <param name="payrollRunId">Optional custom payroll run ID</param>
    /// <param name="employeeId">Optional custom employee ID</param>
    /// <param name="isLocked">Whether the record should be locked</param>
    /// <returns>A dummy MedicalAidDeduction object</returns>
    public static MedicalAidDeduction CreateMedicalAidDeduction(int? id = null, int? payrollRunId = null, string? employeeId = null, bool isLocked = false)
    {
        var recordId = id ?? new Random().Next(1000, 9999);
        var runId = payrollRunId ?? 1;
        var empId = employeeId ?? "EMP001";
        
        return new MedicalAidDeduction
        {
            Id = recordId,
            PayrollRunId = runId,
            IsLocked = isLocked,
            EmployeeId = empId,
            Name = "John",
            Surname = "Doe",
            Branch = "Johannesburg",
            Salary = 50000.00m,
            EmployeeStartDate = new DateTime(2020, 1, 15),
            EffectiveDate = new DateTime(2024, 4, 1),
            TerminationDate = null,
            TerminationReason = null,
            MedicalOptionId = 1,
            MedicalCategoryId = 1,
            OptionName = "Essential Plus",
            OptionCategoryName = "Essential",
            PrincipalCount = 1,
            AdultCount = 1,
            ChildrenCount = 2,
            PrincipalPremium = 1500.00m,
            SpousePremium = 750.00m,
            ChildPremium = 900.00m,
            TotalDeductionAmount = 3150.00m,
            IsActive = true,
            CreatedDate = new DateTime(2024, 3, 25),
            UpdatedDate = DateTime.Now
        };
    }

    /// <summary>
    /// Creates a terminated MedicalAidDeduction record
    /// </summary>
    /// <param name="id">Optional custom ID</param>
    /// <param name="payrollRunId">Optional custom payroll run ID</param>
    /// <param name="employeeId">Optional custom employee ID</param>
    /// <returns>A terminated MedicalAidDeduction object</returns>
    public static MedicalAidDeduction CreateTerminatedMedicalAidDeduction(int? id = null, int? payrollRunId = null, string? employeeId = null)
    {
        var deduction = CreateMedicalAidDeduction(id, payrollRunId, employeeId);
        deduction.IsActive = false;
        deduction.TerminationDate = new DateTime(2024, 3, 31);
        deduction.TerminationReason = "Employee resignation";
        deduction.UpdatedDate = new DateTime(2024, 3, 31);
        return deduction;
    }

    /// <summary>
    /// Creates a collection of medical aid deductions for comprehensive testing
    /// </summary>
    /// <param name="payrollRunId">The payroll run ID to associate records with</param>
    /// <param name="count">Number of records to create</param>
    /// <returns>A list of MedicalAidDeduction objects</returns>
    public static List<MedicalAidDeduction> CreateMedicalAidDeductions(int payrollRunId, int count = 5)
    {
        var records = new List<MedicalAidDeduction>();
        var random = new Random();

        for (int i = 1; i <= count; i++)
        {
            var employeeId = $"EMP{i:D3}";
            var isLocked = random.Next(0, 2) == 1;
            
            records.Add(CreateMedicalAidDeduction(
                id: i,
                payrollRunId: payrollRunId,
                employeeId: employeeId,
                isLocked: isLocked));
        }

        return records;
    }

    /// <summary>
    /// Creates medical aid deductions for different plan categories
    /// </summary>
    /// <param name="payrollRunId">The payroll run ID</param>
    /// <returns>A list of MedicalAidDeduction objects with different categories</returns>
    public static List<MedicalAidDeduction> CreateMedicalAidDeductionsByCategory(int payrollRunId)
    {
        return new List<MedicalAidDeduction>
        {
            new MedicalAidDeduction
            {
                Id = 1,
                PayrollRunId = payrollRunId,
                IsLocked = false,
                EmployeeId = "EMP001",
                Name = "John",
                Surname = "Doe",
                Branch = "Johannesburg",
                Salary = 50000.00m,
                EmployeeStartDate = new DateTime(2020, 1, 15),
                EffectiveDate = new DateTime(2024, 4, 1),
                MedicalOptionId = 1,
                MedicalCategoryId = 1,
                OptionName = "Essential Plus",
                OptionCategoryName = "Essential",
                PrincipalCount = 1,
                AdultCount = 1,
                ChildrenCount = 2,
                PrincipalPremium = 1500.00m,
                SpousePremium = 750.00m,
                ChildPremium = 900.00m,
                TotalDeductionAmount = 3150.00m,
                IsActive = true,
                CreatedDate = new DateTime(2024, 3, 25),
                UpdatedDate = DateTime.Now
            },
            new MedicalAidDeduction
            {
                Id = 2,
                PayrollRunId = payrollRunId,
                IsLocked = false,
                EmployeeId = "EMP002",
                Name = "Jane",
                Surname = "Smith",
                Branch = "Cape Town",
                Salary = 35000.00m,
                EmployeeStartDate = new DateTime(2019, 6, 10),
                EffectiveDate = new DateTime(2024, 4, 1),
                MedicalOptionId = 2,
                MedicalCategoryId = 2,
                OptionName = "Vital Plan",
                OptionCategoryName = "Vital",
                PrincipalCount = 0, // Vital plans don't have principals
                AdultCount = 1,
                ChildrenCount = 1,
                PrincipalPremium = 0.00m,
                SpousePremium = 400.00m,
                ChildPremium = 250.00m,
                TotalDeductionAmount = 650.00m,
                IsActive = true,
                CreatedDate = new DateTime(2024, 3, 25),
                UpdatedDate = DateTime.Now
            },
            new MedicalAidDeduction
            {
                Id = 3,
                PayrollRunId = payrollRunId,
                IsLocked = false,
                EmployeeId = "EMP003",
                Name = "Mike",
                Surname = "Johnson",
                Branch = "Durban",
                Salary = 60000.00m,
                EmployeeStartDate = new DateTime(2018, 3, 20),
                EffectiveDate = new DateTime(2024, 4, 1),
                MedicalOptionId = 3,
                MedicalCategoryId = 3,
                OptionName = "Double Plan",
                OptionCategoryName = "Double",
                PrincipalCount = 0, // Double plans don't have principals
                AdultCount = 2,
                ChildrenCount = 0,
                PrincipalPremium = 0.00m,
                SpousePremium = 1400.00m, // 700 * 2 adults
                ChildPremium = 0.00m,
                TotalDeductionAmount = 1400.00m,
                IsActive = true,
                CreatedDate = new DateTime(2024, 3, 25),
                UpdatedDate = DateTime.Now
            }
        };
    }

    /// <summary>
    /// Creates minimal payroll records for basic testing
    /// </summary>
    /// <param name="payrollRunId">The payroll run ID</param>
    /// <param name="employeeId">The employee ID</param>
    /// <returns>A minimal MedicalAidDeduction object</returns>
    public static MedicalAidDeduction CreateMinimalMedicalAidDeduction(int payrollRunId, string employeeId = "EMP001")
    {
        return new MedicalAidDeduction
        {
            Id = 1,
            PayrollRunId = payrollRunId,
            IsLocked = false,
            EmployeeId = employeeId,
            Name = "Test",
            Surname = "User",
            IsActive = true,
            CreatedDate = DateTime.Now,
            UpdatedDate = DateTime.Now
        };
    }
}
