namespace HRConnect.Api.Interfaces
{
    using System.Threading.Tasks;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Models;
    public interface ILeaveRuleService
    {
        Task UpdateLeaveEntitlementRuleAsync(UpdateLeaveRuleRequest request);
        Task RecalculateEmployeesForRuleChangeAsync(int ruleId);
    }
}