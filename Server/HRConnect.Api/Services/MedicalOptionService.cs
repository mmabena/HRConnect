namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces;

  public class MedicalOptionService:IMedicalOptionService
  {
    // TODO: Implement methods
    private readonly IMedicalOptionRepository _medicalOptionRepository;

    public MedicalOptionService(IMedicalOptionRepository medicalOptionRepository)
    {
      _medicalOptionRepository = medicalOptionRepository;
    }
  }  
}