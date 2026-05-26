namespace HRConnect.Api.Interfaces
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs.OccupationalLevel;
  using HRConnect.Api.Models;


  public interface IOccupationalLevelService

  {
    Task<List<OccupationalLevelDto>> GetAllOccupationalLevelsAsync();

    Task<OccupationalLevelDto?> GetOccupationalLevelByIdAsync(int id);

    Task<OccupationalLevelDto> AddOccupationalLevelAsync(CreateOccupationalLevelDto createOccupationalLevelDto);

    Task<OccupationalLevelDto?> UpdateOccupationalLevelAsync(int id, UpdateOccupationalLevelDto updateOccupationalLevelDto);
  }
}
