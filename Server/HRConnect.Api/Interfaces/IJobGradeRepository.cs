namespace HRConnect.Api.Interfaces
{
    using HRConnect.Api.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IJobGradeRepository
    {
        Task<List<JobGrade>> GetAllJobGradesAsync();

        Task<JobGrade?> GetJobGradeByIdAsync(int id);

        Task<JobGrade?> GetJobGradeByNameAsync(string name);

        Task<JobGrade> AddJobGradeAsync(JobGrade jobGrade);

        Task<JobGrade?> UpdateJobGradeAsync(JobGrade jobGrade);
       
    }
}