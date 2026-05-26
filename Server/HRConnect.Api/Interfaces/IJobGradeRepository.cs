namespace HRConnect.Api.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;

    public interface IJobGradeRepository
    {
        Task<List<JobGrade>> GetAllJobGradesAsync();

        Task<JobGrade?> GetJobGradeByIdAsync(int id);

        Task<JobGrade?> GetJobGradeByNameAsync(string name);

        Task<JobGrade> AddJobGradeAsync(JobGrade jobGrade);

        Task<JobGrade?> UpdateJobGradeAsync(JobGrade jobGrade);
       
    }
}