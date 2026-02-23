namespace HRConnect.Api.Repository
{
     using HRConnect.Api.Data;
     using HRConnect.Api.Interfaces;
     using HRConnect.Api.Models;
     using Microsoft.EntityFrameworkCore;

    public class PositionRepository : IPositionRepository
    {
        private readonly ApplicationDBContext _context;

        public PositionRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<Position>> GetAllPositionsAsync()
        {
            return await _context.Positions
                .Include(p => p.JobGrade)
                .Include(p => p.OccupationalLevels)
                .ToListAsync();
        }

        public async Task<Position?> GetPositionByIdAsync(int id)
        {
            return await _context.Positions
                .Include(p => p.JobGrade)
                .Include(p => p.OccupationalLevels)
                .FirstOrDefaultAsync(p => p.PositionId == id);
        }

        public async Task<Position?> GetPositionByTitleAsync(string title)
        {
            return await _context.Positions
                .Include(p => p.JobGrade)
                .Include(p => p.OccupationalLevels)
                .FirstOrDefaultAsync(p => p.PositionTitle == title);
        }

        public async Task<bool> TitleExistsAsync(string title, int excludeId = 0)
        {
            return await _context.Positions
                .AnyAsync(p => p.PositionTitle == title && p.PositionId != excludeId);
        }

        public async Task<Position> AddPositionAsync(Position position)
        {
            _context.Positions.Add(position);
            await _context.SaveChangesAsync();
            return position;
        }

        public async Task<Position?> UpdatePositionAsync(int id, Position position)
        {
            var existingPosition = await _context.Positions
                .FirstOrDefaultAsync(p => p.PositionId == id);

            if (existingPosition == null)
                return null;

            existingPosition.PositionTitle = position.PositionTitle;
            existingPosition.JobGradeId = position.JobGradeId;
            existingPosition.OccupationalLevelId = position.OccupationalLevelId;
            existingPosition.IsActive = position.IsActive;
            existingPosition.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingPosition;
        }

        
    }
}
