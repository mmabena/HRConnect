namespace HRConnect.Api.Repository
{
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Handles data access operations for Position entities.
    /// This repository interacts directly with the database using Entity Framework.
    /// </summary>
    public class PositionRepository : IPositionRepository
    {

        private readonly ApplicationDBContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionRepository"/> class.
        /// </summary>
        /// <param name="context">The database context used to access data.</param>
        public PositionRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all positions from the database, including related JobGrade and OccupationalLevel data.
        /// </summary>
        /// <returns>A list of all positions.</returns>
        public async Task<List<Position>> GetAllPositionsAsync()
        {
            return await _context.Positions
                .Include(p => p.JobGrade)
                .Include(p => p.OccupationalLevels)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a single position by its unique identifier.
        /// </summary>
        /// <param name="id">The position identifier.</param>
        /// <returns>The position if found; otherwise null.</returns>
        public async Task<Position?> GetPositionByIdAsync(int id)
        {
            return await _context.Positions
                .Include(p => p.JobGrade)
                .Include(p => p.OccupationalLevels)
                .FirstOrDefaultAsync(p => p.PositionId == id);
        }

        /// <summary>
        /// Retrieves a position by its title.
        /// </summary>
        /// <param name="title">The title of the position.</param>
        /// <returns>The position if found; otherwise null.</returns>
        public async Task<Position?> GetPositionByTitleAsync(string title)
        {
            return await _context.Positions
                .Include(p => p.JobGrade)
                .Include(p => p.OccupationalLevels)
                .FirstOrDefaultAsync(p => p.PositionTitle == title);
        }

        /// <summary>
        /// Checks whether a position title already exists in the database.
        /// </summary>
        /// <param name="title">The position title to check.</param>
        /// <param name="excludeId">An optional position ID to exclude (used during updates).</param>
        /// <returns>True if the title exists; otherwise false.</returns>
        public async Task<bool> TitleExistsAsync(string title, int excludeId = 0)
        {
            return await _context.Positions
                .AnyAsync(p => p.PositionTitle == title && p.PositionId != excludeId);
        }

        /// <summary>
        /// Adds a new position to the database.
        /// </summary>
        /// <param name="position">The position entity to add.</param>
        /// <returns>The created position.</returns>
        public async Task<Position> AddPositionAsync(Position position)
        {
            _context.Positions.Add(position);
            await _context.SaveChangesAsync();
            return position;
        }

        /// <summary>
        /// Updates an existing position in the database.
        /// </summary>
        /// <param name="id">The position identifier.</param>
        /// <param name="position">The updated position entity.</param>
        /// <returns>The updated position if successful.</returns>
        public async Task<Position?> UpdatePositionAsync(int id, Position position)
        {
            _context.Positions.Update(position);
            await _context.SaveChangesAsync();
            return position;
        }
    }
}
