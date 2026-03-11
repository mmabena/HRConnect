namespace HRConnect.Api.Interfaces
{
  using DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;
  using Models.PayrollContribution;

  public interface IMedicalAidDeductionService
  {
    Task<MedicalAidDeductionDto> GetMedicalAidDeductionsByEmployeeIdAsync(string employeeId);
    
    Task<IReadOnlyList<MedicalAidDeduction>> GetAllMedicalAidDeductions();
    Task<MedicalAidDeductionDto> AddNewMedicalAidDeductions(string employeeId, int medicalOptionId);
    Task<MedicalAidDeductionDto> UpdateDeductionByEmpId(string employeeId);
  }
}