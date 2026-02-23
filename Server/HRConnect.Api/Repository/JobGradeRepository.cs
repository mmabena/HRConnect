namespace HRConnect.Api.Repository
{
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using HRConnect.Api.Data;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    public class JobGradeRepository : IJobGradeRepository
    {
        private readonly ApplicationDBContext _context;

        public JobGradeRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<JobGrade>> GetAllJobGradesAsync()
        {
            return await _context.JobGrades.ToListAsync();
        }

        public async Task<JobGrade?> GetJobGradeByIdAsync(int id)
        {
            return await _context.JobGrades.FindAsync(id);
        }

         public async Task<JobGrade?> GetJobGradeByNameAsync(string name)
        {
            var jobGrade = await _context.JobGrades
            .FirstOrDefaultAsync(j => j.Name == name);
            return jobGrade;
        }


        public async Task<JobGrade> AddJobGradeAsync(JobGrade jobGrade)
        {
            _context.JobGrades.Add(jobGrade);
            await _context.SaveChangesAsync();
            return jobGrade;
        }

        public async Task<JobGrade?> UpdateJobGradeAsync(JobGrade jobGrade)
        {
          _context.JobGrades.Update(jobGrade);
          await _context.SaveChangesAsync();
          return jobGrade;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
