namespace HRConnect.Api.Repository
{
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using System.Threading.Tasks;
    /// <summary>
    /// Data access for LeaveEntitlementRule entity.
    /// </summary>
    public class LeaveEntitlementRepository : ILeaveEntitlementRepository
    {
        private readonly ApplicationDBContext _context;
        public LeaveEntitlementRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task AddAsync(LeaveEntitlementRule entitlement)
        {
            await _context.LeaveEntitlementRules.AddAsync(entitlement);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}