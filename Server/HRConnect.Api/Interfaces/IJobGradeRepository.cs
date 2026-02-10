
namespace HRConnect.Api.Interfaces
{
    using HRConnect.Api.Models;
    using System.Threading.Tasks;
    public interface IJobGradeRepository
    {
        Task<JobGrade?> GetLatestByEmployeeIdAsync(int employeeId);
        Task AddAsync(JobGrade jobGrade);
        Task SaveChangesAsync();
    }
}