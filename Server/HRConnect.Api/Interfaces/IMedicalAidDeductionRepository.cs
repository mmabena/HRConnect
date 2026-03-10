namespace HRConnect.Api.Interfaces
{
  using Models.PayrollContribution;

  public interface IMedicalAidDeductionRepository
  {
    Task<List<MedicalAidDeduction>> GetMedicalAidDeductionsByEmployeeIdAsync(string employeeId);
    Task<IReadOnlyList<MedicalAidDeduction>> GetAllMedicalAidDeductionsAsync();
    Task AddNewMedicalAidDeductionsAsync(string employeeId);
    Task UpdateDeductionByEmpIdAsync(string employeeId);
  }
}

