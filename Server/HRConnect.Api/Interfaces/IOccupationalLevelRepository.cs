namespace HRConnect.Api.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    public interface IOccupationalLevelRepository
    {
        Task<List<OccupationalLevel>> GetAllOccupationalLevelsAsync();

        Task<OccupationalLevel?> GetOccupationalLevelByIdAsync(int id);

        Task<OccupationalLevel?> GetOccupationalLevelByDescriptionAsync(string description);

        Task AddOccupationalLevelAsync(OccupationalLevel occupationalLevel);

        Task UpdateOccupationalLevelAsync(OccupationalLevel occupationalLevel);
    }
}
