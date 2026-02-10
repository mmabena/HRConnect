
namespace HRConnect.Api.Interfaces
{
    using HRConnect.Api.Models;
    public interface ILeaveEntitlementRepository
    {
        Task AddAsync(LeaveEntitlementRule entitlement);
        Task SaveChangesAsync();
    }
}