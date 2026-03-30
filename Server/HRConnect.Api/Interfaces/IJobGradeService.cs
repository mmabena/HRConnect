namespace HRConnect.Api.Interfaces
{
    using HRConnect.Api.DTOs.JobGrade;
    using System.Collections.Generic;
    using System.Threading.Tasks;   

    public interface IJobGradeService
    {
        Task<List<JobGradeDto>> GetAllJobGradesAsync();
        Task<JobGradeDto?> GetJobGradeByIdAsync(int id);
        Task<JobGradeDto> AddJobGradeAsync(CreateJobGradeDto createJobGradeDto);
        Task<JobGradeDto?> EditJobGradeAsync(int id, UpdateJobGradeDto updateJobGradeDto);
    }
}
