namespace HRConnect.Api.Services;

using Interfaces;

public class MedicalAidDeductionService:IMedicalAidDeductionService
{
  private readonly IMedicalAidDeductionRepository _medicalAidDeductionRepository;
  public MedicalAidDeductionService(IMedicalAidDeductionRepository medicalAidDeductionRepository)
  {
    _medicalAidDeductionRepository = medicalAidDeductionRepository;
  }
  public async Task<> GetMedicalAidDeductions(string employeeId)
  {
    //ToDo: validations
    //ToDo : Fix Signature ASAP
    //return _medicalAidDeductionRepository.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId);
  }

  public async Task GetAllMedicalAidDeductions()
  {
    throw new NotImplementedException();
  }

  public async Task AddNewMedicalAidDeductions(string employeeId)
  {
    throw new NotImplementedException();
  }

  public async Task UpdateDeductionByEmpId(string employeeId, int id)
  {
    throw new NotImplementedException();
  }
}