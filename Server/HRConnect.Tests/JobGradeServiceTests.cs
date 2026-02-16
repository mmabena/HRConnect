namespace HRConnect.Tests.Services
{
  using Moq;
  using HRConnect.Api.Services;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
    using System.Reflection;

    public class JobGradeServiceTests
  {
    private readonly Mock<IJobGradeRepository> _jobGradeRepoMock;
    private readonly JobGradeService _jobGradeService;

    public JobGradeServiceTests()
    {
      _jobGradeRepoMock = new Mock<IJobGradeRepository>();
      _jobGradeService = new JobGradeService(_jobGradeRepoMock.Object);
    }

    [Fact]
    public async Task GetAllJobGradesAsyncReturnsJobGrades()
    {
      // Arrange
      var jobGrades = new List<JobGrade>
      {
        new JobGrade { JobGradeId = 1, Name = "Junior" },
        new JobGrade { JobGradeId = 2, Name = "Senior" }
      };
      _jobGradeRepoMock.Setup(r => r.GetAllJobGradesAsync())
                       .ReturnsAsync(jobGrades);

      // Act
      var result = await _jobGradeService.GetAllJobGradesAsync();
      var list = result.ToList();

      // Assert
      Assert.NotNull(result);
      Assert.Equal(2, result.Count);
      Assert.Equal("Junior", list[0].Name);
    }

    [Fact]
    public async Task GetJobGradeByIdAsyncReturnsJobGrade()
    {
      // Arrange
      var jobGrade = new JobGrade { JobGradeId = 1, Name = "Junior" };
      _jobGradeRepoMock.Setup(r => r.GetJobGradeByIdAsync(1))
                       .ReturnsAsync(jobGrade);

      // Act
      var result = await _jobGradeService.GetJobGradeByIdAsync(1);

      // Assert
      Assert.NotNull(result);
      Assert.Equal("Junior", result.Name);
    }
    [Fact]
    public async Task AddJobGradeAsyncAddsJobGrade()
    {
      // Arrange
      var newJobGrade = new JobGrade { JobGradeId = 3, Name = "Lead" };
      _jobGradeRepoMock.Setup(r => r.AddJobGradeAsync(It.IsAny<JobGrade>()))
                       .ReturnsAsync(newJobGrade);

      // Act
      var result = await _jobGradeService.AddJobGradeAsync(createJobGradeDto: new Api.DTOs.JobGrade.CreateJobGradeDto { Name = "Lead" });

      // Assert
      Assert.NotNull(result);
      Assert.Equal("Lead", result.Name);
    }

    [Fact]
    public async Task EditJobGradeAsyncUpdatesJobGrade()
    {
      // Arrange
      var existingJobGrade = new JobGrade
       { 
        JobGradeId = 1,
        Name = "Junior",
        IsActive = true,
       };

      var updatedJobGrade = new JobGrade 
      {
         JobGradeId = 1,
         Name = "Junior Updated"
     };

        _jobGradeRepoMock
            .Setup(r => r.GetJobGradeByIdAsync(1))
            .ReturnsAsync(existingJobGrade);

      _jobGradeRepoMock.Setup(r => r.UpdateJobGradeAsync(1, It.IsAny<JobGrade>()))
                       .ReturnsAsync(existingJobGrade);

      // Act
      var result = await _jobGradeService.EditJobGradeAsync(1, updateJobGradeDto: new Api.DTOs.JobGrade.UpdateJobGradeDto { Name = "Junior Updated" });

      // Assert
      Assert.NotNull(result);
      Assert.Equal("Junior Updated", result.Name);
    }
  }
}