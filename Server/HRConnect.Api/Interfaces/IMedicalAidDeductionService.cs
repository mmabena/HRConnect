namespace HRConnect.Api.Interfaces
{
  using DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;
  using Models.PayrollDeduction;

  /// <summary>
  /// Defines the contract for medical aid deduction operations.
  /// </summary>
  public interface IMedicalAidDeductionService
  {
    /// <summary>
    /// Get medical aid deductions for a specific employee.
    /// </summary>
    Task<MedicalAidDeductionDto> GetMedicalAidDeductionsByEmployeeIdAsync(string employeeId);
    
    /// <summary>
    /// Get all medical aid deductions.
    /// </summary>
    Task<IReadOnlyList<MedicalAidDeduction>> GetAllMedicalAidDeductions();

    /// <summary>
    /// Add a new medical aid deduction for an employee with selected option details.
    /// </summary>
    /// <param name="employeeId">The employee ID.</param>
    /// <param name="medicalOptionId">The selected medical option ID.</param>
    /// <param name="request">The request containing dependent counts and premium details.</param>
    /// <returns>The created medical aid deduction DTO.</returns>
    Task<MedicalAidDeductionDto> AddNewMedicalAidDeductions(
        string employeeId,
        int medicalOptionId,
        CreateMedicalAidDeductionRequestDto request);

    /// <summary>
    /// Update a medical aid deduction by employee ID.
    /// </summary>
    Task<MedicalAidDeductionDto> UpdateDeductionByEmpId(string employeeId);
  }
}