namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public interface IEmployeeEntitlementService
    {
        Task InitializeEmployeeLeaveBalancesAsync(Guid employeeId);
        Task RecalculateAnnualLeaveAsync(Guid employeeId);
    }
}