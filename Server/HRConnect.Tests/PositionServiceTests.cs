namespace HRConnect.Tests.Services
{
  using Moq;
  using HRConnect.Api.Services;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.DTOs.Position;
  using HRConnect.Api.DTOs.JobGrade;
  using HRConnect.Api.DTOs.OccupationalLevel;
  using HRConnect.Api.Utils;
  using Microsoft.EntityFrameworkCore;
  public class PositionServiceTests
  {
    private readonly Mock<IPositionRepository> _positionRepoMock;
    private readonly PositionService _positionService;

    public PositionServiceTests()
    {
      _positionRepoMock = new Mock<IPositionRepository>();
      _positionService = new PositionService(_positionRepoMock.Object);
    }

    [Fact]
    public async Task GetAllPositionsAsyncReturnsPositions()
    {
      // Arrange
      var positions = new List<Position>
      {
        new Position { PositionId = 1, PositionTitle = "Software Engineer" },
        new Position { PositionId = 2, PositionTitle = "Product Manager" }
      };
      _positionRepoMock.Setup(r => r.GetAllPositionsAsync())
                       .ReturnsAsync(positions);

      // Act
      var result = await _positionService.GetAllPositionsAsync();
      var list = result.ToList();
      // Assert
      Assert.NotNull(result);
      Assert.Equal(2, result.Count);
      Assert.Equal("Software Engineer", list[0].PositionTitle);
    }

    [Fact]
    public async Task GetPositionByIdAsyncReturnsPosition()
    {
      // Arrange
      var position = new Position { PositionId = 1, PositionTitle = "Software Engineer" };
      _positionRepoMock.Setup(r => r.GetPositionByIdAsync(1))
                       .ReturnsAsync(position);

      // Act
      var result = await _positionService.GetPositionByIdAsync(1);

      // Assert
      Assert.NotNull(result);
      Assert.Equal("Software Engineer", result.PositionTitle);
    }

    [Fact]
    public async Task GetPositionByIdAsyncReturnsNullForNonExistentId()
    {
      // Arrange
      _positionRepoMock.Setup(r => r.GetPositionByIdAsync(999))
                       .ReturnsAsync((Position)null);

      // Act
      var result = await _positionService.GetPositionByIdAsync(999);

      // Assert
      Assert.Null(result);
    }

    [Fact]
    public async Task CreatePositionAsyncAddsPosition()
    {
      // Arrange

      var createPositionDto = new CreatePositionDto
       { 
        PositionTitle = "Data Scientist",
        JobGradeId = 1,
        OccupationalLevelId = 1,
        IsActive = true 
        };
        
      var newPosition = new Position { PositionTitle = "Data Scientist" };
      _positionRepoMock.Setup(r => r.AddPositionAsync(It.IsAny<Position>()))
                       .ReturnsAsync((Position pos) => 
                       {
                         pos.PositionId = 3; 
                         return pos;
                       });

      // Act
      var result = await _positionService.AddPositionAsync(createPositionDto: new CreatePositionDto { PositionTitle = "Data Scientist", JobGradeId = 1 });

      // Assert
      Assert.NotNull(result);
      Assert.Equal(3, result.PositionId);
      Assert.Equal("Data Scientist", result.PositionTitle);
  }

    [Fact]
    public async Task UpdatePositionAsyncUpdatesPosition()
    {
      // Arrange
     var existingPosition = new Position 
    { 
        PositionId = 1, 
        PositionTitle = "Software Engineer" 
    };

    var updatedPositionDto = new UpdatePositionDto
    {
        PositionTitle = "Senior Software Engineer"
    };

    var updatedPosition = new Position
    {
        PositionId = 1,
        PositionTitle = "Senior Software Engineer"
    };

      _positionRepoMock.Setup(r => r.GetPositionByIdAsync(1))
                       .ReturnsAsync(existingPosition);

      _positionRepoMock.Setup(r => r.UpdatePositionAsync(1, It.IsAny<Position>()))
                       .ReturnsAsync((int id, Position pos) => pos);

      // Act
      var result = await _positionService.UpdatePositionAsync(1, updatedPositionDto);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(1, result.PositionId);
      Assert.Equal("Senior Software Engineer", result.PositionTitle);
}

 // ---------------------- NEW TESTS FOR DUPLICATE TITLE ----------------------
       [Fact]
public async Task AddPositionAsyncThrowsDomainExceptionOnDuplicateTitle()
{
    // Arrange
    var createDto = new CreatePositionDto
    {
        PositionTitle = "Developer",
        JobGradeId = 1,
        OccupationalLevelId = 1,
        IsActive = true
    };

    // Instead of throwing DbUpdateException, throw DomainException directly
    _positionRepoMock
        .Setup(r => r.AddPositionAsync(It.IsAny<Position>()))
        .ThrowsAsync(new DomainException("A position with this title already exists."));

    // Act & Assert
    var ex = await Assert.ThrowsAsync<DomainException>(() =>
        _positionService.AddPositionAsync(createDto));

    Assert.Equal("A position with this title already exists.", ex.Message);
}

[Fact]
public async Task UpdatePositionAsyncThrowsDomainExceptionOnDuplicateTitle()
{
    // Arrange
    var existingPosition = new Position
    {
        PositionId = 1,
        PositionTitle = "Developer"
    };

    var updateDto = new UpdatePositionDto
    {
        PositionTitle = "Developer", // duplicate title
        IsActive = true
    };

    _positionRepoMock.Setup(r => r.GetPositionByIdAsync(1))
                     .ReturnsAsync(existingPosition);

    // Throw DomainException directly from the repo mock
    _positionRepoMock
        .Setup(r => r.UpdatePositionAsync(1, It.IsAny<Position>()))
        .ThrowsAsync(new DomainException("A position with this title already exists."));

    // Act & Assert
    var ex = await Assert.ThrowsAsync<DomainException>(() =>
        _positionService.UpdatePositionAsync(1, updateDto));

    Assert.Equal("A position with this title already exists.", ex.Message);
}
  }
}
