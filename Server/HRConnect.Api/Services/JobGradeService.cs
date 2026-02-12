namespace HRConnect.Api.Services
{

    using HRConnect.Api.DTOs.JobGrade;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Mappers;
    using HRConnect.Api.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    public class JobGradeService : IJobGradeService
    {
        private readonly IJobGradeRepository _jobGradeRepo;

        public JobGradeService(IJobGradeRepository jobGradeRepo)
        {
            _jobGradeRepo = jobGradeRepo;
        }

         // ----------------------
        // GET ALL
        // ----------------------
        public async Task<List<JobGradeDto>> GetAllJobGradesAsync()
        {
            var jobGrades = await _jobGradeRepo.GetAllJobGradesAsync();
            return jobGrades
                .OrderBy(jg => jg.Name)
                .Select(jg => jg.ToJobGradeDto())
                .ToList();
        }

         // ----------------------
        // GET BY ID
        // ----------------------
        public async Task<JobGradeDto?> GetJobGradeByIdAsync(int id)
        {
            var jobGrade = await _jobGradeRepo.GetJobGradeByIdAsync(id);
            return jobGrade?.ToJobGradeDto();
        }

         // ----------------------
        // GET /api/JobGrades/name
        // ----------------------
       public async Task<JobGradeDto> AddJobGradeAsync(CreateJobGradeDto createJobGradeDto)
       {
            // Null check
            ArgumentNullException.ThrowIfNull(createJobGradeDto);

            if (string.IsNullOrWhiteSpace(createJobGradeDto.Name))
                throw new ArgumentException("Name is required.");

            var trimmedName = createJobGradeDto.Name.Trim();

            // Prevent duplicate
            var existing = await _jobGradeRepo.GetJobGradeByNameAsync(trimmedName);
            if (existing != null)
                throw new InvalidOperationException("Job grade already exists.");

            var jobGrade = new JobGrade
            {
                Name = trimmedName,
                IsActive = createJobGradeDto.IsActive,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _jobGradeRepo.AddJobGradeAsync(jobGrade);

            return jobGrade.ToJobGradeDto();
        }

         public async Task<JobGradeDto?> EditJobGradeAsync(int id, UpdateJobGradeDto updateJobGradeDto)
        {
            // Null check
            ArgumentNullException.ThrowIfNull(updateJobGradeDto);

            var jobGrade = await _jobGradeRepo.GetJobGradeByIdAsync(id);
            if (jobGrade == null) return null;

            var trimmedName = updateJobGradeDto.Name.Trim();

            // Prevent duplicate name if changed
            if (!string.Equals(jobGrade.Name, trimmedName, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _jobGradeRepo.GetJobGradeByNameAsync(trimmedName);
                if (existing != null)
                    throw new InvalidOperationException("Job grade already exists.");
            }

            // Update properties
            jobGrade.Name = trimmedName;
            jobGrade.IsActive = updateJobGradeDto.IsActive;
            jobGrade.UpdatedDate = DateTime.UtcNow;

            await _jobGradeRepo.UpdateJobGradeAsync(id, jobGrade);

            return jobGrade.ToJobGradeDto();
        }

    }
}
