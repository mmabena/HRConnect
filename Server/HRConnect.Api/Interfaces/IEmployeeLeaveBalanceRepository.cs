namespace HRConnect.Api.Interfaces
{
    using HRConnect.Api.Models;
    public interface IEmployeeLeaveBalanceRepository
    {
        Task AddAsync(EmployeeLeaveBalance balance);
        Task SaveChanges();
    }
}