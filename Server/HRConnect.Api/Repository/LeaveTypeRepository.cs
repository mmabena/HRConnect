namespace HRConnect.Api.Repository
{
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;
    /// <summary>
    /// Data access for LeaveType entity.
    /// </summary>
    public class LeaveTypeRepository : ILeaveTypeRepository
    {
        private readonly ApplicationDBContext _context;
        public LeaveTypeRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<List<LeaveType>> GetAllAsync()
        {
            return await _context.LeaveTypes.ToListAsync();
        }
    }
}