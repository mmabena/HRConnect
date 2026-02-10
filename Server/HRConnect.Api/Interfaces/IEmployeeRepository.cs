
namespace HRConnect.Api.Interfaces
{
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    using HRConnect.Api.DTOs;
    public interface IEmployeeRepository
    {
        Task<Employee?> GetByIdAsync(int employeeId);
        Task AddAsync(Employee employee);
        Task SaveChangesAsync();
        Task<IEnumerable<LeaveEntitlementResponse>> GetLeaveEntitlementsAsync(int employeeId);
    }
}