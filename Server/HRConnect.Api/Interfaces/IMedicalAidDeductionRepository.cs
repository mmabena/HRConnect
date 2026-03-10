namespace HRConnect.Api.Interfaces
{
  public interface IMedicalAidDeductionRepository
  {
    Task GetMedicalAidDeductions(string employeeId);
    Task GetAllMedicalAidDeductions();
    Task AddNewMedicalAidDeductions(string employeeId);
    Task UpdateDeductionByEmpId(string employeeId, int id);
  }
}

