
namespace HRConnect.Api.Interfaces
{
    using HRConnect.Api.Models;
    using System.Threading.Tasks;
    /// <summary>
    /// Allocates initial leave entitlements when an employee is created.
    /// </summary>
    public interface ILeaveEntitlementService
    {
        Task AllocateOnEmployeeHireAsync(int employeeId);
    }
}