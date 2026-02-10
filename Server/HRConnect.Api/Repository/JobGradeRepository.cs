namespace HRConnect.Api.Repository
{
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    /// <summary>
    /// Data access for JobGrade history.(Promotion/Demotion)
    /// </summary>
    public class JobGradeRepository : IJobGradeRepository
    {
        private readonly ApplicationDBContext _context;
        public JobGradeRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<JobGrade?> GetLatestByEmployeeIdAsync(int employeeId)
        {
            return await _context.JobGrades
            .Where(j => j.EmployeeId == employeeId)
            .OrderByDescending(j => j.CreatedDate)
            .FirstOrDefaultAsync();
        }
        public async Task AddAsync(JobGrade jobGrade)
        {
            await _context.JobGrades.AddAsync(jobGrade);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}