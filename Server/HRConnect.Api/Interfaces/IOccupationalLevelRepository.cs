namespace HRConnect.Api.Interfaces
{
    using HRConnect.Api.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    public interface IOccupationalLevelRepository
    {
        Task<List<OccupationalLevel>> GetAllOccupationalLevelsAsync();

        Task<OccupationalLevel?> GetOccupationalLevelByIdAsync(int id);

        Task<OccupationalLevel?> GetOccupationalLevelByDescriptionAsync(string description);

        Task AddOccupationalLevelAsync(OccupationalLevel occupationalLevel);

        Task UpdateOccupationalLevelAsync(OccupationalLevel occupationalLevel);
    }
}
