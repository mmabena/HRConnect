namespace HRConnect.Tests
{
  using Xunit;
  using Moq;
  using Moq.EntityFrameworkCore;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using Api.Mappers;
  using Api.Utils.MedicalOption;
  using HRConnect.Api.Utils.MedicalOption;
  using HRConnect.Api.DTOs.MedicalOption;
  using HRConnect.Api.Models;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Utils.MedicalOption.Records;
  using System;
  using System.Linq;
  using Api.Services;
  using HRConnect.Api.Middleware;

  public class MedicalOptionServiceTests
  {
    private readonly Mock<IMedicalOptionRepository> _mockRepository;
    private readonly MedicalOptionService _service;

    public MedicalOptionServiceTests()
    {
      _mockRepository = new Mock<IMedicalOptionRepository>();
      _service = new MedicalOptionService(_mockRepository.Object);
    }

    #region Constructor Tests

    [Fact]
    public void ConstructorWithValidRepositoryShouldInitializeService()
    {
      // Arrange
      var repository = new Mock<IMedicalOptionRepository>().Object;

      // Act
      var service = new MedicalOptionService(repository);

      // Assert
      Assert.NotNull(service);
    }

    [Fact]
    public void ConstructorWithNullRepositoryShouldThrowArgumentNullException()
    {
      // Act & Assert
      Assert.Throws<ArgumentNullException>(() => new MedicalOptionService(null));
    }

    #endregion

    #region GetGroupedMedicalOptionsAsync Tests

    [Fact]
    public async Task GetGroupedMedicalOptionsAsyncWithValidDataShouldReturnGroupedOptions()
    {
      var mockData = new List<MedicalOption>
      {
        new() { 
          MedicalOptionId = 1, 
          MedicalOptionName = "Plan A",
          MedicalOptionCategoryId = 1,
          MedicalOptionCategory = new MedicalOptionCategory 
          { 
            MedicalOptionCategoryId = 1, 
            MedicalOptionCategoryName = "Vital" 
          }
        },
        new() { 
          MedicalOptionId = 2, 
          MedicalOptionName = "Plan B",
          MedicalOptionCategoryId = 1,
          MedicalOptionCategory = new MedicalOptionCategory 
          { 
            MedicalOptionCategoryId = 1, 
            MedicalOptionCategoryName = "Vital" 
          }
        }
      };

      // Group the data by MedicalOptionCategoryId to match the expected return type
      var groupedData = mockData.GroupBy(x => x.MedicalOptionCategoryId).ToList();

      _mockRepository.Setup(r => r.GetGroupedMedicalOptionsAsync())
        .ReturnsAsync(groupedData);

      // Act
      var result = await _service.GetGroupedMedicalOptionsAsync();

      // Assert
      Assert.NotNull(result);
      Assert.Single(result);
      Assert.Equal("Vital", result.First().MedicalOptionCategoryName);
      Assert.Equal(2, result.First().MedicalOptions.Count);
      _mockRepository.Verify(r => r.GetGroupedMedicalOptionsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetGroupedMedicalOptionsAsyncWhenRepositoryThrowsShouldPropagateException()
    {
      // Arrange
      _mockRepository.Setup(r => r.GetGroupedMedicalOptionsAsync())
          .ThrowsAsync(new InvalidOperationException("Database error"));

      // Act & Assert
      await Assert.ThrowsAsync<InvalidOperationException>(
          () => _service.GetGroupedMedicalOptionsAsync());
    }

    #endregion

    #region GetMedicalOptionByIdAsync Tests

    [Fact]
    public async Task GetMedicalOptionByIdAsyncWithValidIdShouldReturnOption()
    {
      // Arrange
      var optionId = 1;
      var expectedOption = new MedicalOptionDto 
      { 
          MedicalOptionId = optionId, 
          MedicalOptionName = "Plan A" 
      };

      _mockRepository.Setup(r => r.GetMedicalOptionByIdAsync(optionId))
          .ReturnsAsync(expectedOption);

      // Act
      var result = await _service.GetMedicalOptionByIdAsync(optionId);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(optionId, result.MedicalOptionId);
      _mockRepository.Verify(r => r.GetMedicalOptionByIdAsync(optionId), Times.Once);
    }

    [Fact]
    public async Task GetMedicalOptionByIdAsyncWithInvalidIdShouldReturnNull()
    {
      // Arrange
      var optionId = 999;
      _mockRepository.Setup(r => r.GetMedicalOptionByIdAsync(optionId))
          .ReturnsAsync((MedicalOptionDto?)null);

      // Act
      var result = await _service.GetMedicalOptionByIdAsync(optionId);

      // Assert
      Assert.Null(result);
      _mockRepository.Verify(r => r.GetMedicalOptionByIdAsync(optionId), Times.Once);
    }

    #endregion

    #region GetMedicalOptionCategoryByIdAsync Tests

    [Fact]
    public async Task GetMedicalOptionCategoryByIdAsyncWithValidCategoryIdShouldReturnFirstOption()
    {
      // Arrange
      var categoryId = 1;
      var options = new List<MedicalOptionDto?>
      {
          new() { MedicalOptionId = 1, MedicalOptionName = "Plan A", MedicalOptionCategoryId = categoryId },
          new() { MedicalOptionId = 2, MedicalOptionName = "Plan B", MedicalOptionCategoryId = categoryId }
      };

      _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
          .ReturnsAsync(options);

      // Act
      var result = await _service.GetMedicalOptionCategoryByIdAsync(categoryId);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(1, result.MedicalOptionId);
      _mockRepository.Verify(r => r.GetAllOptionsUnderCategoryAsync(categoryId), Times.Once);
    }

    [Fact]
    public async Task GetMedicalOptionCategoryByIdAsyncWithInvalidCategoryIdShouldThrowKeyNotFoundException()
    {
      // Arrange
      var categoryId = 999;
      _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
          .ReturnsAsync(new List<MedicalOptionDto?>());

      // Act & Assert
      var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
          () => _service.GetMedicalOptionCategoryByIdAsync(categoryId));
      Assert.Contains("no medical option found for category 999", exception.Message);
    }

    #endregion

    #region MedicalOptionCategoryExistsAsync Tests

    [Fact]
    public async Task MedicalOptionCategoryExistsAsyncWithExistingCategoryShouldReturnTrue()
    {
      // Arrange
      var categoryId = 1;
      _mockRepository.Setup(r => r.MedicalOptionCategoryExistsAsync(categoryId))
          .ReturnsAsync(true);

      // Act
      var result = await _service.MedicalOptionCategoryExistsAsync(categoryId);

      // Assert
      Assert.True(result);
      _mockRepository.Verify(r => r.MedicalOptionCategoryExistsAsync(categoryId), Times.Once);
    }

    [Fact]
    public async Task MedicalOptionCategoryExistsAsyncWithNonExistingCategoryShouldReturnFalse()
    {
      // Arrange
      var categoryId = 999;
      _mockRepository.Setup(r => r.MedicalOptionCategoryExistsAsync(categoryId))
          .ReturnsAsync(false);

      // Act
      var result = await _service.MedicalOptionCategoryExistsAsync(categoryId);

      // Assert
      Assert.False(result);
      _mockRepository.Verify(r => r.MedicalOptionCategoryExistsAsync(categoryId), Times.Once);
    }

    #endregion

    #region MedicalOptionExistsAsync Tests

    [Fact]
    public async Task MedicalOptionExistsAsyncWithExistingOptionShouldReturnTrue()
    {
      // Arrange
      var optionId = 1;
      _mockRepository.Setup(r => r.MedicalOptionExistsAsync(optionId))
          .ReturnsAsync(true);

      // Act
      var result = await _service.MedicalOptionExistsAsync(optionId);

      // Assert
      Assert.True(result);
      _mockRepository.Verify(r => r.MedicalOptionExistsAsync(optionId), Times.Once);
    }

    [Fact]
    public async Task MedicalOptionExistsAsyncWithNonExistingOptionShouldReturnFalse()
    {
      // Arrange
      var optionId = 999;
      _mockRepository.Setup(r => r.MedicalOptionExistsAsync(optionId))
          .ReturnsAsync(false);

      // Act
      var result = await _service.MedicalOptionExistsAsync(optionId);

      // Assert
      Assert.False(result);
      _mockRepository.Verify(r => r.MedicalOptionExistsAsync(optionId), Times.Once);
    }

    #endregion

    #region GetAllOptionsUnderCategoryAsync Tests

    [Fact]
    public async Task GetAllOptionsUnderCategoryAsyncWithValidCategoryShouldReturnOptions()
    {
      // Arrange
      var categoryId = 1;
      var options = new List<MedicalOptionDto?>
      {
          new() { MedicalOptionId = 1, MedicalOptionName = "Plan A", MedicalOptionCategoryId = categoryId },
          new() { MedicalOptionId = 2, MedicalOptionName = "Plan B", MedicalOptionCategoryId = categoryId }
      };

      _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
          .ReturnsAsync(options);

      // Act
      var result = await _service.GetAllOptionsUnderCategoryAsync(categoryId);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(2, result.Count);
      _mockRepository.Verify(r => r.GetAllOptionsUnderCategoryAsync(categoryId), Times.Once);
    }

    [Fact]
    public async Task GetAllOptionsUnderCategoryAsyncWithInvalidCategoryShouldReturnEmptyList()
    {
      // Arrange
      var categoryId = 999;
      _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
          .ReturnsAsync(new List<MedicalOptionDto?>());

      // Act
      var result = await _service.GetAllOptionsUnderCategoryAsync(categoryId);

      // Assert
      Assert.NotNull(result);
      Assert.Empty(result);
      _mockRepository.Verify(r => r.GetAllOptionsUnderCategoryAsync(categoryId), Times.Once);
    }

    #endregion

    #region MedicalOptionExistsWithinCategoryAsync Tests

    [Fact]
    public async Task MedicalOptionExistsWithinCategoryAsyncWithValidCombinationShouldReturnTrue()
    {
      // Arrange
      var categoryId = 1;
      var optionId = 1;
      _mockRepository.Setup(r => r.MedicalOptionExistsWithinCategoryAsync(categoryId, optionId))
          .ReturnsAsync(true);

      // Act
      var result = await _service.MedicalOptionExistsWithinCategoryAsync(categoryId, optionId);

      // Assert
      Assert.True(result);
      _mockRepository.Verify(r => r.MedicalOptionExistsWithinCategoryAsync(categoryId, optionId), Times.Once);
    }

    [Fact]
    public async Task MedicalOptionExistsWithinCategoryAsyncWithInvalidCombinationShouldReturnFalse()
    {
      // Arrange
      var categoryId = 1;
      var optionId = 999;
      _mockRepository.Setup(r => r.MedicalOptionExistsWithinCategoryAsync(categoryId, optionId))
          .ReturnsAsync(false);

      // Act
      var result = await _service.MedicalOptionExistsWithinCategoryAsync(categoryId, optionId);

      // Assert
      Assert.False(result);
      _mockRepository.Verify(r => r.MedicalOptionExistsWithinCategoryAsync(categoryId, optionId), Times.Once);
    }

    #endregion

    #region BulkUpdateMedicalOptionsByCategoryAsync Tests

    [Fact]
    public async Task BulkUpdateMedicalOptionsByCategoryAsyncWithValidDataShouldUpdateSuccessfully()
    {
      // Arrange
      var categoryId = 1;
      var bulkUpdateDto = new List<UpdateMedicalOptionVariantsDto>
      {
          new() { 
              MedicalOptionId = 1, 
              SalaryBracketMin = 0,
              SalaryBracketMax = 15000,
              MonthlyRiskContributionPrincipal = null,
              MonthlyRiskContributionAdult = 500,
              MonthlyRiskContributionChild = 300,
              MonthlyRiskContributionChild2 = 0,
              MonthlyMsaContributionPrincipal = null,
              MonthlyMsaContributionAdult = null,
              MonthlyMsaContributionChild = null,
              TotalMonthlyContributionsPrincipal = null,
              TotalMonthlyContributionsAdult = 500,
              TotalMonthlyContributionsChild = 300,
              TotalMonthlyContributionsChild2 = null
          },
          new() { 
              MedicalOptionId = 2, 
              SalaryBracketMin = 15001,
              SalaryBracketMax = 30000,
              MonthlyRiskContributionPrincipal = null,
              MonthlyRiskContributionAdult = 750,
              MonthlyRiskContributionChild = 450,
              MonthlyRiskContributionChild2 = null,
              MonthlyMsaContributionPrincipal = null,
              MonthlyMsaContributionAdult = null,
              MonthlyMsaContributionChild = null,
              TotalMonthlyContributionsPrincipal = null,
              TotalMonthlyContributionsAdult = 750,
              TotalMonthlyContributionsChild = 450,
              TotalMonthlyContributionsChild2 = null
          }
      };
    
      var dbData = new List<MedicalOptionDto>
      {
          new() { 
              MedicalOptionId = 1, 
              MedicalOptionName = "Plan A",
              MedicalOptionCategoryId = categoryId,
              SalaryBracketMin = 0,
              SalaryBracketMax = 15000,
              MonthlyRiskContributionPrincipal = null,
              MonthlyRiskContributionAdult = 500,
              MonthlyRiskContributionChild = 300,
              MonthlyRiskContributionChild2 = null,
              MonthlyMsaContributionPrincipal = null,
              MonthlyMsaContributionAdult = null,
              MonthlyMsaContributionChild = null,
              TotalMonthlyContributionsPrincipal = null,
              TotalMonthlyContributionsAdult = 500,
              TotalMonthlyContributionsChild = 300,
              TotalMonthlyContributionsChild2 = null
          },
          new() { 
              MedicalOptionId = 2, 
              MedicalOptionName = "Plan B",
              MedicalOptionCategoryId = categoryId,
              SalaryBracketMin = 15001,
              SalaryBracketMax = 30000,
              MonthlyRiskContributionPrincipal = null,
              MonthlyRiskContributionAdult = 750,
              MonthlyRiskContributionChild = 450,
              MonthlyRiskContributionChild2 = null,
              MonthlyMsaContributionPrincipal = null,
              MonthlyMsaContributionAdult = null,
              MonthlyMsaContributionChild = null,
              TotalMonthlyContributionsPrincipal = null,
              TotalMonthlyContributionsAdult = 750,
              TotalMonthlyContributionsChild = 450,
              TotalMonthlyContributionsChild2 = null
          }
      };
    
      var updatedOptions = new List<MedicalOptionDto>
      {
          new() { MedicalOptionId = 1, MedicalOptionName = "Plan A Updated" },
          new() { MedicalOptionId = 2, MedicalOptionName = "Plan B Updated" }
      };
      
      // Create a date within update period (Nov 15, 2024)
      var testDate = new DateTime(2024, 11, 15, 12, 0, 0);
    
      // Mock all repository methods that might be called
      _mockRepository.Setup(r => r.GetCategoryByIdAsync(categoryId))
          .ReturnsAsync(new MedicalOptionCategory 
          { 
              MedicalOptionCategoryId = categoryId,
              MedicalOptionCategoryName = "Vital" // Non-restricted category
          });
      
      _mockRepository.Setup(r => r.MedicalOptionCategoryExistsAsync(categoryId))
          .ReturnsAsync(true);
      
      _mockRepository.Setup(r => r.MedicalOptionExistsAsync(It.IsAny<int>()))
          .ReturnsAsync(true);
      
      _mockRepository.Setup(r => r.MedicalOptionExistsWithinCategoryAsync(It.IsAny<int>(), It.IsAny<int>()))
          .ReturnsAsync(true);
    
      _mockRepository.Setup(r => r.GetMedicalOptionsByIdsAsync(It.IsAny<List<int>>()))
          .ReturnsAsync(new List<MedicalOptionDto>
          {
              new() { MedicalOptionId = 1, MedicalOptionName = "Plan A" },
              new() { MedicalOptionId = 2, MedicalOptionName = "Plan B" }
          });
    
      _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
          .ReturnsAsync(dbData);
    
      _mockRepository.Setup(r => r.BulkUpdateByCategoryIdAsync(categoryId, bulkUpdateDto))
          .ReturnsAsync(updatedOptions);
    
      // Act
      var result = await _service.BulkUpdateMedicalOptionsByCategoryAsync(categoryId, bulkUpdateDto,testDate);
    
      // Assert
      Assert.NotNull(result);
      Assert.Equal(2, result.Count);
      _mockRepository.Verify(r => r.GetAllOptionsUnderCategoryAsync(categoryId), Times.Once);
      _mockRepository.Verify(r => r.BulkUpdateByCategoryIdAsync(categoryId, bulkUpdateDto), Times.Once);
    }

    [Fact]
    public async Task BulkUpdateMedicalOptionsByCategoryAsyncWithInvalidCategoryIdShouldThrowArgumentException()
    {
      // Arrange
      var invalidCategoryId = 0;
      var bulkUpdateDto = new List<UpdateMedicalOptionVariantsDto>
      {
          new() { MedicalOptionId = 1 }
      };

      // Act & Assert
      var exception = await Assert.ThrowsAsync<ArgumentException>(
          () => _service.BulkUpdateMedicalOptionsByCategoryAsync(invalidCategoryId, bulkUpdateDto));
      Assert.Contains("Category ID must be greater than 0", exception.Message);
    }

    [Fact]
    public async Task BulkUpdateMedicalOptionsByCategoryAsyncWithNullBulkUpdateDtoShouldThrowArgumentException()
    {
      // Arrange
      var categoryId = 1;

      // Act & Assert
      var exception = await Assert.ThrowsAsync<ArgumentException>(
          () => _service.BulkUpdateMedicalOptionsByCategoryAsync(categoryId, null));
      Assert.Contains("Bulk update data cannot be null or empty", exception.Message);
    }

    [Fact]
    public async Task BulkUpdateMedicalOptionsByCategoryAsyncWithEmptyBulkUpdateDtoShouldThrowArgumentException()
    {
      // Arrange
      var categoryId = 1;
      var emptyBulkUpdateDto = new List<UpdateMedicalOptionVariantsDto>();

      // Act & Assert
      var exception = await Assert.ThrowsAsync<ArgumentException>(
          () => _service.BulkUpdateMedicalOptionsByCategoryAsync(categoryId, emptyBulkUpdateDto));
      Assert.Contains("Bulk update data cannot be null or empty", exception.Message);
    }

    [Fact]
    public async Task BulkUpdateMedicalOptionsByCategoryAsyncWithValidationFailureShouldThrowValidationException()
    {
      // Arrange
      var categoryId = 1;
      var bulkUpdateDto = new List<UpdateMedicalOptionVariantsDto>
      {
          new() { MedicalOptionId = 1 }, // Missing required contribution data
          new() { MedicalOptionId = 2 }
      };

      var dbData = new List<MedicalOptionDto>
      {
          new() { 
              MedicalOptionId = 1, 
              MedicalOptionName = "Plan A",
              MedicalOptionCategoryId = categoryId,
              MonthlyRiskContributionPrincipal = 1000,
              TotalMonthlyContributionsPrincipal = 1000
          }
      };

      _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
          .ReturnsAsync(dbData);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Api.Middleware.ValidationException>(
          () => _service.BulkUpdateMedicalOptionsByCategoryAsync(categoryId, bulkUpdateDto));
      Assert.NotNull(exception);
    }

    [Fact]
    public async Task BulkUpdateMedicalOptionsByCategoryAsyncWithRestrictedCategorySalaryUpdateShouldThrowValidationException()
    {
      // Arrange
      var categoryId = 1;
      var bulkUpdateDto = new List<UpdateMedicalOptionVariantsDto>
      {
        new() { 
          MedicalOptionId = 1, 
          SalaryBracketMin = 1000, // Salary update on restricted category
          MonthlyRiskContributionPrincipal = 1000,
          TotalMonthlyContributionsPrincipal = 1000
        }
      };

      // The DTOs returned by repository - these will be converted to entities in the service
      var dbData = new List<MedicalOptionDto>
      {
        new() { 
          MedicalOptionId = 1, 
          MedicalOptionName = "Alliance Plus",
          MedicalOptionCategoryId = categoryId,
          MonthlyRiskContributionPrincipal = 1000,
          TotalMonthlyContributionsPrincipal = 1000
        }
      };

      _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
        .ReturnsAsync(dbData);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Api.Middleware.ValidationException>(
        () => _service.BulkUpdateMedicalOptionsByCategoryAsync(categoryId, bulkUpdateDto));
  
      
      Console.WriteLine($"Actual exception message: {exception.Message}");
  
      
      Assert.NotNull(exception);
    }

    [Fact]
    public async Task BulkUpdateMedicalOptionsByCategoryAsyncWithNonExistentIdsShouldThrowValidationException()
    {
      // Arrange
      var categoryId = 1;
      var bulkUpdateDto = new List<UpdateMedicalOptionVariantsDto>
      {
          new() { 
              MedicalOptionId = 1,
              MonthlyRiskContributionPrincipal = 1000,
              TotalMonthlyContributionsPrincipal = 1000
          },
          new() { 
              MedicalOptionId = 999, // Non-existent ID
              MonthlyRiskContributionPrincipal = 1500,
              TotalMonthlyContributionsPrincipal = 1500
          }
      };

      var dbData = new List<MedicalOptionDto>
      {
          new() { 
              MedicalOptionId = 1, 
              MedicalOptionName = "Plan A",
              MedicalOptionCategoryId = categoryId,
              MonthlyRiskContributionPrincipal = 1000,
              TotalMonthlyContributionsPrincipal = 1000
          }
      };

      // Create a date within update period (Nov 15, 2024)
      var testDate = new DateTime(2024, 11, 15, 12, 0, 0);
      
      _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
          .ReturnsAsync(dbData);

      // Act & Assert
      var exception = await Assert.ThrowsAsync<Api.Middleware.ValidationException>(
          () => _service.BulkUpdateMedicalOptionsByCategoryAsync(categoryId, bulkUpdateDto, testDate));
      Assert.Contains("do not exist", exception.Message);
    }

    [Fact]
    public async Task BulkUpdateMedicalOptionsByCategoryAsyncWithRepositoryExceptionShouldPropagateException()
    {
      // Arrange
      var categoryId = 1;
      var bulkUpdateDto = new List<UpdateMedicalOptionVariantsDto>
      {
          new() { 
              MedicalOptionId = 1,
              MonthlyRiskContributionPrincipal = 1000,
              TotalMonthlyContributionsPrincipal = 1000
          }
      };

      _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
          .ThrowsAsync(new InvalidOperationException("Database error"));

      // Act & Assert
      await Assert.ThrowsAsync<InvalidOperationException>(
          () => _service.BulkUpdateMedicalOptionsByCategoryAsync(categoryId, bulkUpdateDto));
    }

    #endregion

        #region CreateBulkOptionsByExistingCategoryId Tests

    [Fact]
    public async Task CreateBulkOptionsByExistingCategoryIdWithValidDataShouldInsertSuccessfully()
    {
        // Arrange
        var categoryId = 1;
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new()
            {
                MedicalOptionName = "Essential Plus",
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 0,
                SalaryBracketMax = 15000,
                MonthlyRiskContributionPrincipal = 1000,
                MonthlyRiskContributionAdult = 500,
                MonthlyRiskContributionChild = 300,
                MonthlyMsaContributionPrincipal = 500,
                MonthlyMsaContributionAdult = 250,
                MonthlyMsaContributionChild = 150,
                TotalMonthlyContributionsPrincipal = 1500,
                TotalMonthlyContributionsAdult = 750,
                TotalMonthlyContributionsChild = 450
            },
            new()
            {
                MedicalOptionName = "Essential Standard",
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 15001,
                SalaryBracketMax = 30000,
                MonthlyRiskContributionPrincipal = 1200,
                MonthlyRiskContributionAdult = 600,
                MonthlyRiskContributionChild = 350,
                MonthlyMsaContributionPrincipal = 600,
                MonthlyMsaContributionAdult = 300,
                MonthlyMsaContributionChild = 175,
                TotalMonthlyContributionsPrincipal = 1800,
                TotalMonthlyContributionsAdult = 900,
                TotalMonthlyContributionsChild = 525
            }
        };

        var categoryInfo = new List<MedicalOptionCategory>
        {
            new()
            {
                MedicalOptionCategoryId = categoryId,
                MedicalOptionCategoryName = "Essential"
            }
        };

        var existingOptions = new List<MedicalOptionDto>(); // Empty - new options
        var insertedOptions = new List<CreateMedicalOptionVariantsDto>(bulkInsertDto);
        var testDate = new DateTime(2024, 11, 15);

        // Mock all repository methods
        _mockRepository.Setup(r => r.GetCategoryById(categoryId))
            .ReturnsAsync(categoryInfo);

        _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
            .ReturnsAsync(existingOptions);

        _mockRepository.Setup(r => r.CreateBulkOptionsByExistingCategoryId(categoryId, bulkInsertDto))
            .ReturnsAsync(insertedOptions);

        // Act
        var result = await _service.CreateBulkOptionsByExistingCategoryId(categoryId, bulkInsertDto, testDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        _mockRepository.Verify(r => r.GetCategoryById(categoryId), Times.Once);
        _mockRepository.Verify(r => r.GetAllOptionsUnderCategoryAsync(categoryId), Times.Once);
        _mockRepository.Verify(r => r.CreateBulkOptionsByExistingCategoryId(categoryId, bulkInsertDto), Times.Once);
    }

    [Fact]
    public async Task CreateBulkOptionsByExistingCategoryIdWithInvalidCategoryIdShouldThrowArgumentException()
    {
        // Arrange
        var invalidCategoryId = 0;
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new() { MedicalOptionName = "Essential Plus", MedicalOptionCategoryId = invalidCategoryId }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateBulkOptionsByExistingCategoryId(invalidCategoryId, bulkInsertDto));
        Assert.Contains("Category ID must be greater than 0", exception.Message);
    }

    [Fact]
    public async Task CreateBulkOptionsByExistingCategoryIdWithNullPayloadShouldThrowArgumentException()
    {
        // Arrange
        var categoryId = 1;
        List<CreateMedicalOptionVariantsDto>? nullPayload = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateBulkOptionsByExistingCategoryId(categoryId, nullPayload!));
        Assert.Contains("cannot be null or empty", exception.Message);
    }

    [Fact]
    public async Task CreateBulkOptionsByExistingCategoryIdWithEmptyPayloadShouldThrowArgumentException()
    {
        // Arrange
        var categoryId = 1;
        var emptyPayload = new List<CreateMedicalOptionVariantsDto>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateBulkOptionsByExistingCategoryId(categoryId, emptyPayload));
        Assert.Contains("cannot be null or empty", exception.Message);
    }

    [Fact]
    public async Task CreateBulkOptionsByExistingCategoryIdWithNonExistentCategoryShouldThrowInvalidOperationException()
    {
        // Arrange
        var categoryId = 999;
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new() { MedicalOptionName = "Essential Plus", MedicalOptionCategoryId = categoryId }
        };

        _mockRepository.Setup(r => r.GetCategoryById(categoryId))
            .ReturnsAsync(new List<MedicalOptionCategory>()); // Empty list - category not found

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateBulkOptionsByExistingCategoryId(categoryId, bulkInsertDto));
        Assert.Contains("does not exist", exception.Message);
    }

    [Fact]
    public async Task CreateBulkOptionsByExistingCategoryIdWithMismatchedCategoryIdShouldThrowArgumentException()
    {
        // Arrange
        var categoryId = 1;
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new()
            {
                MedicalOptionName = "Essential Plus",
                MedicalOptionCategoryId = 999 // Wrong category ID
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateBulkOptionsByExistingCategoryId(categoryId, bulkInsertDto));
        Assert.Contains("incorrect category ID", exception.Message);
    }

    [Fact]
    public async Task CreateBulkOptionsByExistingCategoryIdShouldThrowValidationExceptionWhenValidationFails()
    {
        // Arrange
        var categoryId = 1;
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new()
            {
                MedicalOptionName = "Invalid Name", // Does not contain category name
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 0,
                SalaryBracketMax = 15000,
                MonthlyRiskContributionAdult = 500,
                MonthlyRiskContributionChild = 300,
                TotalMonthlyContributionsAdult = 500,
                TotalMonthlyContributionsChild = 300
            }
        };

        var categoryInfo = new List<MedicalOptionCategory>
        {
            new()
            {
                MedicalOptionCategoryId = categoryId,
                MedicalOptionCategoryName = "Essential"
            }
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        _mockRepository.Setup(r => r.GetCategoryById(categoryId))
            .ReturnsAsync(categoryInfo);

        _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
            .ReturnsAsync(existingOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateBulkOptionsByExistingCategoryId(categoryId, bulkInsertDto, testDate));
        Assert.Contains("must contain the category name", exception.Message);
    }

    [Fact]
    public async Task CreateBulkOptionsByExistingCategoryIdOutsideUpdatePeriodShouldThrowValidationException()
    {
        // Arrange
        var categoryId = 1;
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new()
            {
                MedicalOptionName = "Essential Plus",
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 0,
                SalaryBracketMax = 15000,
                MonthlyRiskContributionAdult = 500,
                MonthlyRiskContributionChild = 300,
                TotalMonthlyContributionsAdult = 500,
                TotalMonthlyContributionsChild = 300
            }
        };

        var categoryInfo = new List<MedicalOptionCategory>
        {
            new()
            {
                MedicalOptionCategoryId = categoryId,
                MedicalOptionCategoryName = "Essential"
            }
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 9, 15); // September - outside update period

        _mockRepository.Setup(r => r.GetCategoryById(categoryId))
            .ReturnsAsync(categoryInfo);

        _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
            .ReturnsAsync(existingOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateBulkOptionsByExistingCategoryId(categoryId, bulkInsertDto, testDate));
        Assert.Contains("November and December", exception.Message);
    }

    [Fact]
    public async Task CreateBulkOptionsByExistingCategoryIdShouldPassWithTestDateParameter()
    {
        // Arrange
        var categoryId = 1;
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new()
            {
                MedicalOptionName = "Vital Plus",
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 0,
                SalaryBracketMax = 15000,
                MonthlyRiskContributionAdult = 500,
                MonthlyRiskContributionChild = 300,
                TotalMonthlyContributionsAdult = 500,
                TotalMonthlyContributionsChild = 300
            }
        };

        var categoryInfo = new List<MedicalOptionCategory>
        {
            new()
            {
                MedicalOptionCategoryId = categoryId,
                MedicalOptionCategoryName = "Vital"
            }
        };

        var existingOptions = new List<MedicalOptionDto>();
        var insertedOptions = new List<CreateMedicalOptionVariantsDto>(bulkInsertDto);

        // Test with December date
        var testDate = new DateTime(2024, 12, 15);

        _mockRepository.Setup(r => r.GetCategoryById(categoryId))
            .ReturnsAsync(categoryInfo);

        _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
            .ReturnsAsync(existingOptions);

        _mockRepository.Setup(r => r.CreateBulkOptionsByExistingCategoryId(categoryId, bulkInsertDto))
            .ReturnsAsync(insertedOptions);

        // Act
        var result = await _service.CreateBulkOptionsByExistingCategoryId(categoryId, bulkInsertDto, testDate);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        _mockRepository.Verify(r => r.CreateBulkOptionsByExistingCategoryId(categoryId, bulkInsertDto), Times.Once);
    }

    [Fact]
    public async Task CreateBulkOptionsByExistingCategoryIdWithDuplicateOptionNamesShouldThrowValidationException()
    {
        // Arrange
        var categoryId = 1;
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new()
            {
                MedicalOptionName = "Essential Plus",
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 0,
                SalaryBracketMax = 10000,
                MonthlyRiskContributionAdult = 500,
                MonthlyRiskContributionChild = 300,
                TotalMonthlyContributionsAdult = 500,
                TotalMonthlyContributionsChild = 300
            },
            new()
            {
                MedicalOptionName = "Essential Plus", // Duplicate name
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 10001,
                SalaryBracketMax = 20000,
                MonthlyRiskContributionAdult = 600,
                MonthlyRiskContributionChild = 350,
                TotalMonthlyContributionsAdult = 600,
                TotalMonthlyContributionsChild = 350
            }
        };

        var categoryInfo = new List<MedicalOptionCategory>
        {
            new()
            {
                MedicalOptionCategoryId = categoryId,
                MedicalOptionCategoryName = "Essential"
            }
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        _mockRepository.Setup(r => r.GetCategoryById(categoryId))
            .ReturnsAsync(categoryInfo);

        _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
            .ReturnsAsync(existingOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateBulkOptionsByExistingCategoryId(categoryId, bulkInsertDto, testDate));
        Assert.Contains("Duplicate option names detected", exception.Message);
    }

    [Fact]
    public async Task CreateBulkOptionsByExistingCategoryIdWithExistingOptionNameShouldThrowValidationException()
    {
        // Arrange
        var categoryId = 1;
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new()
            {
                MedicalOptionName = "Essential Plus",
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 0,
                SalaryBracketMax = 15000,
                MonthlyRiskContributionAdult = 500,
                MonthlyRiskContributionChild = 300,
                TotalMonthlyContributionsAdult = 500,
                TotalMonthlyContributionsChild = 300
            }
        };

        var categoryInfo = new List<MedicalOptionCategory>
        {
            new()
            {
                MedicalOptionCategoryId = categoryId,
                MedicalOptionCategoryName = "Essential"
            }
        };

        // Existing option with same name
        var existingOptions = new List<MedicalOptionDto>
        {
            new()
            {
                MedicalOptionId = 1,
                MedicalOptionName = "Essential Plus",
                MedicalOptionCategoryId = categoryId
            }
        };

        var testDate = new DateTime(2024, 11, 15);

        _mockRepository.Setup(r => r.GetCategoryById(categoryId))
            .ReturnsAsync(categoryInfo);

        _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
            .ReturnsAsync(existingOptions);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateBulkOptionsByExistingCategoryId(categoryId, bulkInsertDto, testDate));
        Assert.Contains("already exists in category", exception.Message);
    }

    #endregion
  }
}