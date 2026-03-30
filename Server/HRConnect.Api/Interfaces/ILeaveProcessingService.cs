namespace HRConnect.Api.Interfaces
{
    using System.Threading.Tasks;
    public interface ILeaveProcessingService
    {
        Task RecalculateAllSickLeaveAsync();
        Task RecalculateAllFamilyResponsibilityLeaveAsync();
        Task ProcessCarryOverNotificationAsync();
        Task ProcessAnnualResetAsync(int? overrideYear = null);
    }
}