namespace HRConnect.Api.Services;

using DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;
using Interfaces;
using Models.PayrollDeduction;

public class MedicalAidDeductionService:IMedicalAidDeductionService
{
  private readonly IMedicalAidDeductionRepository _medicalAidDeductionRepository;
  public MedicalAidDeductionService(IMedicalAidDeductionRepository medicalAidDeductionRepository)
  {
    _medicalAidDeductionRepository = medicalAidDeductionRepository;
  }
  public async Task<MedicalAidDeductionDto> GetMedicalAidDeductionsByEmployeeIdAsync(string employeeId)
  {
    //ToDo: validations
    //ToDo : Fix Signature ASAP
    var employeeDeductions =
      _medicalAidDeductionRepository.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId);
    throw new NotImplementedException();
    //return _medicalAidDeductionRepository.GetMedicalAidDeductionsByEmployeeIdAsync(employeeId);
  }

  public async Task<IReadOnlyList<MedicalAidDeduction>> GetAllMedicalAidDeductions()
  {
    throw new NotImplementedException();
  }

  public async Task<MedicalAidDeductionDto> AddNewMedicalAidDeductions(string employeeId,
    int medicalOptionId)
  {
    throw new NotImplementedException();
  }

  public async Task<MedicalAidDeductionDto> UpdateDeductionByEmpId(string employeeId)
  {
    throw new NotImplementedException();
  }
}