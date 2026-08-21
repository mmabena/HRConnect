namespace HRConnect.Api.Interfaces
{
  using Models.PayrollDeduction;

  public interface IMedicalAidDeductionRepository
  {
    Task<List<MedicalAidDeduction>> GetMedicalAidDeductionsByEmployeeIdAsync(string employeeId);
    Task<IReadOnlyList<MedicalAidDeduction>> GetAllMedicalAidDeductionsAsync();
    Task<IReadOnlyList<MedicalAidDeduction>> GetAllLockedMedicalAidDeductionsAsync();
    Task AddNewMedicalAidDeductionsAsync(MedicalAidDeduction deduction);

    Task UpdateDeductionsByEmpIdAsync(string employeeId, int payrollRunId,
      MedicalAidDeduction updatePayloadDeduction);
    
    Task<MedicalAidDeduction?> GetActiveMedicalAidDeductionByEmpIdAsync(string employeeId);
    Task<List<MedicalAidDeduction>> GetActiveDeductionsFromMostRecentFinalizedRunAsync();
    Task TerminateMedicalAidDeductionAsync(MedicalAidDeduction terminateDeduction);
    Task<MedicalAidDeduction?> GetMedicalAidDeductionForCurrentRunAsync(string employeeId);
    Task<List<MedicalAidDeduction>> GetActiveDeductionsByPayrollRunIdAsync(int payrollRunId);

    Task<List<MedicalAidDeduction>> GetAllRecordsFromPreviousRun(int previousRunNumber);

    Task SaveChangesAsync();

    Task<MedicalAidDeduction?> GetMedicalAidDeductionByEmployeeAndPayrollRunAsync(string employeeId, int payrollRunId);

  }
}

