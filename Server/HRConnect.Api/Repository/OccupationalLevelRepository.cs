namespace HRConnect.Api.Repository
{
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    public class OccupationalLevelRepository : IOccupationalLevelRepository
    {
        private readonly ApplicationDBContext _context;

        public OccupationalLevelRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<OccupationalLevel>> GetAllOccupationalLevelsAsync()
        {
            return await _context.OccupationalLevels.ToListAsync();
        }

        public async Task<OccupationalLevel?> GetOccupationalLevelByIdAsync(int id)
        {
            return await _context.OccupationalLevels
                .FirstOrDefaultAsync(o => o.OccupationalLevelId == id);
        }

        public async Task<OccupationalLevel?> GetOccupationalLevelByDescriptionAsync(string description)
        {
            return await _context.OccupationalLevels
                .FirstOrDefaultAsync(x => x.Description == description);
        }

        public async Task AddOccupationalLevelAsync(OccupationalLevel occupationalLevel)
        {
            await _context.OccupationalLevels.AddAsync(occupationalLevel);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOccupationalLevelAsync(OccupationalLevel occupationalLevel)
        {
            _context.OccupationalLevels.Update(occupationalLevel);
            await _context.SaveChangesAsync();
        }
    }
}
