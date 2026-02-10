namespace HRConnect.Api.Repository
{
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using System.Threading.Tasks;
    /// <summary>
    /// Data access for EmployeeLeaveBalance entity.
    /// </summary>
    public class EmployeeLeaveBalanceRepository : IEmployeeLeaveBalanceRepository
    {
        private readonly ApplicationDBContext _context;
        public EmployeeLeaveBalanceRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task AddAsync(EmployeeLeaveBalance balance)
        {
            await _context.EmployeeLeaveBalances.AddAsync(balance);
        }
        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}