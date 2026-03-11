namespace HRConnect.Api.Interfaces
{
  using Models.PayrollDeduction;

  public interface IMedicalAidDeductionRepository
  {
    Task<List<MedicalAidDeduction>> GetMedicalAidDeductionsByEmployeeIdAsync(string employeeId);
    Task<IReadOnlyList<MedicalAidDeduction>> GetAllMedicalAidDeductionsAsync();
    Task AddNewMedicalAidDeductionsAsync(string employeeId);
    Task UpdateDeductionByEmpIdAsync(string employeeId);
  }
}

