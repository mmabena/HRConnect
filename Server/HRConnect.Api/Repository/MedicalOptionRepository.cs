namespace HRConnect.Api.Repository
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;
  
  public class MedicalOptionRepository: IMedicalOptionRepository
  {
    private readonly ApplicationDBContext _context;
    
    public MedicalOptionRepository(ApplicationDBContext context)
    {
      _context = context;
    }
    
    public async Task<List<MedicalOption>> GetAllMedicalOptionsGroupedByCategoryAsync()
    {
      //TODO: Implement method
      
      //TODO: Extract the logic to a service
      return null;
    }

  }
}