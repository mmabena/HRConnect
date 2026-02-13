namespace HRConnect.Tests.Services
{
  using Moq;
  using HRConnect.Api.Services;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.OccupationalLevel;
  public class OccupationalLevelServiceTests
  {
    private readonly Mock<IOccupationalLevelRepository> _occupationalLevelRepoMock;
    private readonly OccupationalLevelService _occupationalLevelService;

    public OccupationalLevelServiceTests()
    {
      _occupationalLevelRepoMock = new Mock<IOccupationalLevelRepository>();
      _occupationalLevelService = new OccupationalLevelService(_occupationalLevelRepoMock.Object);
    }

    [Fact]
    public async Task GetAllOccupationalLevelsAsyncReturnsOccupationalLevels()
    {
      // Arrange
      var occupationalLevels = new List<OccupationalLevel>
      {
        new OccupationalLevel { OccupationalLevelId = 1, Description = "Entry Level" },
        new OccupationalLevel { OccupationalLevelId = 2, Description = "Mid Level" }
      };
      _occupationalLevelRepoMock.Setup(r => r.GetAllOccupationalLevelsAsync())
                       .ReturnsAsync(occupationalLevels);

      // Act
      var result = await _occupationalLevelService.GetAllOccupationalLevelsAsync();
      var list = result.ToList();

      // Assert
      Assert.NotNull(result);
      Assert.Equal(2, result.Count);
      Assert.Equal("Entry Level", list[0].Description);
      Assert.Equal("Mid Level", list[1].Description);
    }

    [Fact]
    public async Task AddOccupationalLevelAsyncAddsOccupationalLevel()
    {
      // Arrange
 var newOccupationalLevel = new OccupationalLevel { OccupationalLevelId = 3, Description = "Senior Level" };

    _occupationalLevelRepoMock
    .Setup(o => o.AddOccupationalLevelAsync(It.IsAny<OccupationalLevel>()))
    .Returns(Task.CompletedTask); 


      // Act
      var result = await _occupationalLevelService.AddOccupationalLevelAsync(createOccupationalLevelDto: new CreateOccupationalLevelDto { Description = "Senior Level" });

      // Assert
      Assert.NotNull(result);
      Assert.Equal("Senior Level", result.Description);
  }

[Fact]
public async Task UpdateOccupationalLevelAsyncUpdatesOccupationalLevel()
{
    // Arrange
    var existingOccupationalLevel = new OccupationalLevel
    {
        OccupationalLevelId = 1,
        Description = "Entry Level"
    };

    var updatedOccupationalLevel = new OccupationalLevel
    {
        OccupationalLevelId = 1,
        Description = "Entry Level Updated"
    };

    _occupationalLevelRepoMock
        .Setup(r => r.GetOccupationalLevelByIdAsync(1))
        .ReturnsAsync(existingOccupationalLevel);

    _occupationalLevelRepoMock
        .Setup(r => r.UpdateOccupationalLevelAsync(It.IsAny<OccupationalLevel>()))
        .Returns(Task.CompletedTask); // repo returns Task, not Task<T>

    // Act
    var result = await _occupationalLevelService.UpdateOccupationalLevelAsync(1, updateOccupationalLevelDto: new UpdateOccupationalLevelDto { OccupationalLevelId = 1, Description = "Entry Level Updated" });

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Entry Level Updated", result.Description);
}

  }
}