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
  using Utils;

  public class MedicalOptionValidatorTests
   {
     private readonly Mock<IMedicalOptionRepository> _mockRepository;

     public MedicalOptionValidatorTests()
     {
         _mockRepository = new Mock<IMedicalOptionRepository>();
     }

     // Update Period Tests
     [Fact]
     public void ValidateUpdatePeriodShouldReturnTrueDuringNovemberDecember()
     {
         // Arrange
         var testDate = new DateTime(2024, 11, 15); // November
         
         // Act
         var result = MedicalOptionValidator.ValidateUpdatePeriod(testDate);
         
         // Assert
         Assert.True(result);
     }

     [Fact]
     public void ValidateUpdatePeriodShouldReturnFalseOutsideNovemberDecember()
     {
         // Arrange
         var testDate = new DateTime(2024, 09, 15); // September
         
         // Act
         var result = MedicalOptionValidator.ValidateUpdatePeriod(testDate);
         
         // Assert
         Assert.False(result);
     }

     // Entity Count Tests
     [Fact]
     public void ValidateEntityCountShouldReturnTrueWhenCountsMatch()
     {
         // Arrange
         var dbCount = 5;
         var payloadCount = 5;
         
         // Act
         var result = MedicalOptionValidator.ValidateEntityCount(dbCount, payloadCount);
         
         // Assert
         Assert.True(result);
     }

     [Fact]
     public void ValidateEntityCountShouldReturnFalseWhenCountsDontMatch()
     {
         // Arrange
         var dbCount = 5;
         var payloadCount = 3;
         
         // Act
         var result = MedicalOptionValidator.ValidateEntityCount(dbCount, payloadCount);
         
         // Assert
         Assert.False(result);
     }

     // ID Existence Tests
     [Fact]
     public async Task ValidateAllIdsExistAsyncShouldReturnTrueForValidIds()
     {
         // Arrange
         var bulkUpdateDto = new List<UpdateMedicalOptionVariantsDto>
         {
             new() { MedicalOptionId = 1 },
             new() { MedicalOptionId = 2 }
         };
         
         var dbData = new List<MedicalOption>
         {
             new() { MedicalOptionId = 1 },
             new() { MedicalOptionId = 2 },
             new() { MedicalOptionId = 3 }
         };
         
         // Act
         var result = MedicalOptionValidator.ValidateAllIdsExistAsync(bulkUpdateDto, _mockRepository.Object, dbData);
         
         // Assert
         Assert.True(result);
     }

     [Fact]
     public async Task ValidateAllIdsExistAsyncShouldReturnFalseForInvalidIds()
     {
         // Arrange
         var bulkUpdateDto = new List<UpdateMedicalOptionVariantsDto>
         {
             new() { MedicalOptionId = 1 },
             new() { MedicalOptionId = 999 } // Invalid ID
         };
         
         var dbData = new List<MedicalOption>
         {
             new() { MedicalOptionId = 1 },
             new() { MedicalOptionId = 2 }
         };
         
         // Act
         var result = MedicalOptionValidator.ValidateAllIdsExistAsync(bulkUpdateDto, _mockRepository.Object, dbData);
         
         // Assert
         Assert.False(result);
     }

     // Category Membership Tests
     [Fact]
     public async Task ValidateAllIdsInCategoryAsyncShouldReturnTrueForValidCategoryIds()
     {
         // Arrange
         var categoryId = 1;
         var bulkUpdateDto = new List<UpdateMedicalOptionVariantsDto>
         {
             new() { MedicalOptionId = 1 },
             new() { MedicalOptionId = 2 }
         };
         
         var dbData = new List<MedicalOption>
         {
             new() { MedicalOptionId = 1, MedicalOptionCategoryId = 1 },
             new() { MedicalOptionId = 2, MedicalOptionCategoryId = 1 },
             new() { MedicalOptionId = 3, MedicalOptionCategoryId = 2 }
         };
         
         // Act
         var result = MedicalOptionValidator.ValidateAllIdsInCategoryAsync(bulkUpdateDto, categoryId, _mockRepository.Object, dbData);
         
         // Assert
         Assert.True(result);
     }

     [Fact]
     public async Task ValidateAllIdsInCategoryAsyncShouldReturnFalseForWrongCategoryIds()
     {
         // Arrange
         var categoryId = 1;
         var bulkUpdateDto = new List<UpdateMedicalOptionVariantsDto>
         {
             new() { MedicalOptionId = 1 },
             new() { MedicalOptionId = 3 } // Wrong category
         };
         
         var dbData = new List<MedicalOption>
         {
             new() { MedicalOptionId = 1, MedicalOptionCategoryId = 1 },
             new() { MedicalOptionId = 2, MedicalOptionCategoryId = 1 },
             new() { MedicalOptionId = 3, MedicalOptionCategoryId = 2 }
         };
         
         // Act
         var result = MedicalOptionValidator.ValidateAllIdsInCategoryAsync(bulkUpdateDto, categoryId, _mockRepository.Object, dbData);
         
         // Assert
         Assert.False(result);
     }

     // Salary Bracket Restriction Tests
     [Fact]
     public void ValidateSalaryBracketRestrictionShouldReturnTrueForUnrestrictedCategoryWithSalary()
     {
         // Arrange
         var categoryName = "Choice"; // Not restricted
         var salaryMin = 1000m;
         var salaryMax = 5000m;
         
         // Act
         var result = MedicalOptionValidator.ValidateSalaryBracketRestriction(categoryName, salaryMin, salaryMax);
         
         // Assert
         Assert.True(result);
     }

     [Fact]
     public void ValidateSalaryBracketRestrictionShouldReturnFalseForRestrictedCategoryWithSalary()
     {
         // Arrange
         var categoryName = "Alliance"; // Restricted
         var salaryMin = 1000m;
         var salaryMax = 5000m;
         
         // Act
         var result = MedicalOptionValidator.ValidateSalaryBracketRestriction(categoryName, salaryMin, salaryMax);
         
         // Assert
         Assert.False(result);
     }

     // Salary Range Gap Tests
     [Fact]
     public void ValidateNoGapsInSalaryRangesShouldReturnTrueForContiguousRanges()
     {
         // Arrange
         var records = new List<SalaryBracketValidatorRecord>
         {
             new(1, "Option1", 0, 5000),
             new(2, "Option2", 5001, 10000)
         };
         
         // Act
         var result = MedicalOptionValidator.ValidateNoGapsInSalaryRanges(records);
         
         // Assert
         Assert.True(result);
     }

     [Fact]
     public void ValidateNoGapsInSalaryRangesShouldReturnFalseForGaps()
     {
         // Arrange
         var records = new List<SalaryBracketValidatorRecord>
         {
             new(1, "Option1", 0, 5000),
             new(2, "Option2", 6000, 10000) // Gap of 999
         };
         
         // Act
         var result = MedicalOptionValidator.ValidateNoGapsInSalaryRanges(records);
         
         // Assert
         Assert.False(result);
     }

     // Overlapping Brackets Tests
     [Fact]
     public void ValidateNoOverlappingBracketsShouldReturnTrueForNonOverlappingRanges()
     {
         // Arrange
         var records = new List<SalaryBracketValidatorRecord>
         {
             new(1, "Option1", 0, 5000),
             new(2, "Option2", 5001, 10000)
         };
         
         // Act
         var result = MedicalOptionValidator.ValidateNoOverlappingBrackets(records);
         
         // Assert
         Assert.True(result);
     }

     [Fact]
     public void ValidateNoOverlappingBracketsShouldReturnFalseForOverlappingRanges()
     {
         // Arrange
         var records = new List<SalaryBracketValidatorRecord>
         {
             new(1, "Option1", 0, 5000),
             new(2, "Option2", 4000, 8000) // Overlap
         };
         
         // Act
         var result = MedicalOptionValidator.ValidateNoOverlappingBrackets(records);
         
         // Assert
         Assert.False(result);
     }

     // Contribution Validation Tests
     [Fact]
     public void ValidateContributionValuesWithContextShouldReturnTrueForValidContributions()
     {
         // Arrange
         var entity = new UpdateMedicalOptionVariantsDto
         {
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
         };
         
         var dbOption = new MedicalOption
         {
           MedicalOptionId = 1, 
           MedicalOptionName = "Plan A",
           MedicalOptionCategoryId = 1,
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
         };
         
         // Act
         var result = MedicalOptionValidator.ValidateContributionValuesWithContext(entity, dbOption);
         
         // Assert
         Assert.True(result);
     }

     [Fact]
     public void ValidateContributionValuesWithContextShouldReturnFalseForInvalidContributions()
     {
         // Arrange
         var entity = new UpdateMedicalOptionVariantsDto
         {
             MonthlyRiskContributionPrincipal = -1000 // Invalid negative value
         };
         
         var dbOption = new MedicalOption
         {
             MonthlyRiskContributionPrincipal = 1000
         };
         
         // Act
         var result = MedicalOptionValidator.ValidateContributionValuesWithContext(entity, dbOption);
         
         // Assert
         Assert.False(result);
     }

     [Fact]
     public async Task ValidateAllCategoryVariantsComprehensiveAsyncShouldPassValidPayload()
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
         
         var dbData = new List<MedicalOption>
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

         // Create a date within update period (Nov 15, 2024)
         var testDate = new DateTime(2024, 11, 15, 12, 0, 0);
         
         var updatedOptions = new List<MedicalOptionDto>
         {
           new() { MedicalOptionId = 1, MedicalOptionName = "Plan A Updated" },
           new() { MedicalOptionId = 2, MedicalOptionName = "Plan B Updated" }
         };
    
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
           .ReturnsAsync(dbData.Select(option => option.ToMedicalOptionDto()).ToList());
    
         _mockRepository.Setup(r => r.BulkUpdateByCategoryIdAsync(categoryId, bulkUpdateDto))
           .ReturnsAsync(updatedOptions);
         
         // Act
         var result = await MedicalOptionValidator.ValidateAllCategoryVariantsComprehensiveAsync(
             categoryId, bulkUpdateDto, _mockRepository.Object, dbData, testDate);
         
         // Assert
         Assert.True(result.IsValid);
     }

     [Fact]
     public async Task ValidateAllCategoryVariantsComprehensiveAsyncShouldFailForInvalidIds()
     {
         // Arrange
         var categoryId = 1;
         var bulkUpdateDto = new List<UpdateMedicalOptionVariantsDto>
         {
             new() { MedicalOptionId = 1 },
             new() { MedicalOptionId = 999 } // Invalid ID
         };
         
         var dbData = new List<MedicalOption>
         {
             new() { MedicalOptionId = 1, MedicalOptionName = "Alliance Plus", MedicalOptionCategoryId = 1 }
         };
         
         // Create a date within update period (Nov 15, 2024)
         var testDate = new DateTime(2024, 11, 15, 12, 0, 0);
         
         _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
                    .ReturnsAsync(dbData.Select(option => option.ToMedicalOptionDto()).ToList());
         
         // Act
         var result = await MedicalOptionValidator.ValidateAllCategoryVariantsComprehensiveAsync(
             categoryId, bulkUpdateDto, _mockRepository.Object, dbData, testDate);
         
         // Assert
         Assert.False(result.IsValid);
         Assert.Contains("do not exist", result.ErrorMessage);
     }

         #region Bulk Insert Validation Tests

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldPassForValidNewOptions()
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
                MonthlyRiskContributionChild2 = 0,
                MonthlyMsaContributionPrincipal = 500,
                MonthlyMsaContributionAdult = 250,
                MonthlyMsaContributionChild = 150,
                TotalMonthlyContributionsPrincipal = 1500,
                TotalMonthlyContributionsAdult = 750,
                TotalMonthlyContributionsChild = 450,
                TotalMonthlyContributionsChild2 = null
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
                MonthlyRiskContributionChild2 = null,
                MonthlyMsaContributionPrincipal = 600,
                MonthlyMsaContributionAdult = 300,
                MonthlyMsaContributionChild = 175,
                TotalMonthlyContributionsPrincipal = 1800,
                TotalMonthlyContributionsAdult = 900,
                TotalMonthlyContributionsChild = 525,
                TotalMonthlyContributionsChild2 = null
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Essential"
        };

        var existingOptions = new List<MedicalOptionDto>(); // Empty - new category
        var testDate = new DateTime(2024, 11, 15); // November - valid update period

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldFailOutsideUpdatePeriod()
    {
        // Arrange
        var categoryId = 1;
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new()
            {
                MedicalOptionName = "Essential Plus",
                MedicalOptionCategoryId = categoryId
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Essential"
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 9, 15); // September - invalid update period

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("November and December", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldFailWhenOptionNameDoesNotContainCategoryName()
    {
        // Arrange
        var categoryId = 1;
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new()
            {
                MedicalOptionName = "Invalid Plus", // Does not contain "Essential"
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 0,
                SalaryBracketMax = 15000,
                MonthlyRiskContributionPrincipal = 1000,
                MonthlyRiskContributionAdult = 500,
                MonthlyRiskContributionChild = 300,
                TotalMonthlyContributionsPrincipal = 1500,
                TotalMonthlyContributionsAdult = 750,
                TotalMonthlyContributionsChild = 450
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Essential"
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("must contain the category name", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldFailForDuplicateOptionNamesInPayload()
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
            },
            new()
            {
                MedicalOptionName = "Essential Plus", // Duplicate name
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 15001,
                SalaryBracketMax = 30000,
                MonthlyRiskContributionAdult = 600,
                MonthlyRiskContributionChild = 350,
                TotalMonthlyContributionsAdult = 600,
                TotalMonthlyContributionsChild = 350
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Essential"
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Duplicate option names detected", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldFailWhenOptionNameAlreadyExistsInDatabase()
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

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Essential"
        };

        var existingOptions = new List<MedicalOptionDto>
        {
            new()
            {
                MedicalOptionId = 1,
                MedicalOptionName = "Essential Plus", // Already exists
                MedicalOptionCategoryId = categoryId
            }
        };

        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("already exists in category", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldFailForRestrictedCategoryWithSalaryBrackets()
    {
        // Arrange
        var categoryId = 1;
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new()
            {
                MedicalOptionName = "Alliance Plus",
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 1000, // Should be 0 for restricted category
                SalaryBracketMax = 5000,
                MonthlyRiskContributionAdult = 500,
                MonthlyRiskContributionChild = 300,
                MonthlyMsaContributionAdult = 250,
                MonthlyMsaContributionChild = 150,
                TotalMonthlyContributionsAdult = 750,
                TotalMonthlyContributionsChild = 450
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Alliance" // Restricted category
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Salary bracket restrictions", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldPassForRestrictedCategoryWithZeroMinSalary()
    {
        // Arrange
        var categoryId = 1;
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new()
            {
                MedicalOptionName = "Alliance Plus",
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 0, // Correct for restricted category
                SalaryBracketMax = null, // Unlimited
                MonthlyRiskContributionAdult = 500,
                MonthlyRiskContributionChild = 300,
                MonthlyMsaContributionAdult = 250,
                MonthlyMsaContributionChild = 150,
                TotalMonthlyContributionsAdult = 750,
                TotalMonthlyContributionsChild = 450
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Alliance"
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldFailForGapsInSalaryRanges()
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
                MedicalOptionName = "Essential Standard",
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 20001, // Gap - should be 10001
                SalaryBracketMax = 30000,
                MonthlyRiskContributionAdult = 600,
                MonthlyRiskContributionChild = 350,
                TotalMonthlyContributionsAdult = 600,
                TotalMonthlyContributionsChild = 350
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Essential"
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Gaps detected", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldFailForOverlappingSalaryRanges()
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
                MedicalOptionName = "Essential Standard",
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 5000, // Overlaps with first option
                SalaryBracketMax = 20000,
                MonthlyRiskContributionAdult = 600,
                MonthlyRiskContributionChild = 350,
                TotalMonthlyContributionsAdult = 600,
                TotalMonthlyContributionsChild = 350
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Essential"
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Overlapping", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldFailForInvalidContributionValues()
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
                MonthlyRiskContributionAdult = -500, // Negative - invalid
                MonthlyRiskContributionChild = 300,
                TotalMonthlyContributionsAdult = 500,
                TotalMonthlyContributionsChild = 450
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Essential"
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Invalid contribution values", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldFailWhenRiskPlusMsaNotEqualTotal()
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
                MonthlyMsaContributionAdult = 250,
                MonthlyMsaContributionChild = 150,
                TotalMonthlyContributionsAdult = 1000, // Should be 750 (500+250)
                TotalMonthlyContributionsChild = 450
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Essential"
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldFailForDuplicateContributionsAcrossVariants()
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
                MonthlyRiskContributionPrincipal = 1000, // Same as Plus
                MonthlyRiskContributionAdult = 500,      // Same as Plus
                MonthlyRiskContributionChild = 300,      // Same as Plus
                MonthlyMsaContributionPrincipal = 500,   // Same as Plus
                MonthlyMsaContributionAdult = 250,        // Same as Plus
                MonthlyMsaContributionChild = 150,        // Same as Plus
                TotalMonthlyContributionsPrincipal = 1500, // Same as Plus
                TotalMonthlyContributionsAdult = 750,     // Same as Plus
                TotalMonthlyContributionsChild = 450      // Same as Plus
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Essential"
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("identical contribution values", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldFailForNetworkChoiceVariantWithMsa()
    {
        // Arrange
        var categoryId = 1;
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new()
            {
                MedicalOptionName = "Network Choice Plus",
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 0,
                SalaryBracketMax = 15000,
                MonthlyRiskContributionPrincipal = 1000,
                MonthlyRiskContributionAdult = 500,
                MonthlyRiskContributionChild = 300,
                MonthlyMsaContributionPrincipal = 500, // Should not have MSA
                MonthlyMsaContributionAdult = 250,
                MonthlyMsaContributionChild = 150,
                TotalMonthlyContributionsPrincipal = 1500,
                TotalMonthlyContributionsAdult = 750,
                TotalMonthlyContributionsChild = 450
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Network Choice"
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("should NOT have MSA contributions", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldFailForFirstChoiceVariantWithPrincipal()
    {
        // Arrange
        var categoryId = 1;
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new()
            {
                MedicalOptionName = "First Choice Plus",
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 0,
                SalaryBracketMax = 15000,
                MonthlyRiskContributionPrincipal = 1000, // Should not have Principal
                MonthlyRiskContributionAdult = 500,
                MonthlyRiskContributionChild = 300,
                TotalMonthlyContributionsPrincipal = 1000,
                TotalMonthlyContributionsAdult = 500,
                TotalMonthlyContributionsChild = 300
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "First Choice"
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("should NOT have Principal contributions", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldFailForEssentialVariantWithoutMsa()
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
                // Missing MSA - Essential requires MSA
                TotalMonthlyContributionsPrincipal = 1000,
                TotalMonthlyContributionsAdult = 500,
                TotalMonthlyContributionsChild = 300
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Essential"
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("must have MSA contributions", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldPassForVitalVariant()
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

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Vital"
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldFailForInconsistentMsaStructureWithinVariant()
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
                MonthlyMsaContributionPrincipal = 500, // Has MSA
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
                // Missing MSA - inconsistent with Plus
                TotalMonthlyContributionsPrincipal = 1200,
                TotalMonthlyContributionsAdult = 600,
                TotalMonthlyContributionsChild = 350
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Essential"
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Inconsistent MSA structure", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldFailForInconsistentPrincipalStructureWithinVariant()
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
                MonthlyRiskContributionPrincipal = 1000, // Has Principal
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
                MonthlyRiskContributionAdult = 600, // No Principal - inconsistent
                MonthlyRiskContributionChild = 350,
                MonthlyMsaContributionAdult = 300,
                MonthlyMsaContributionChild = 175,
                TotalMonthlyContributionsAdult = 900,
                TotalMonthlyContributionsChild = 525
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Essential"
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Inconsistent Principal structure", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldFailWhenCategoryInfoIsNull()
    {
        // Arrange
        var categoryId = 999; // Non-existent category
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new()
            {
                MedicalOptionName = "Essential Plus",
                MedicalOptionCategoryId = categoryId
            }
        };

        MedicalOptionCategory? categoryInfo = null; // Null category info
        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo!, existingOptions, testDate);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("not found", result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldPassForDoubleVariantWithMsaAndNoPrincipal()
    {
        // Arrange
        var categoryId = 1;
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new()
            {
                MedicalOptionName = "Double Plus",
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 0,
                SalaryBracketMax = null,
                MonthlyRiskContributionAdult = 500,
                MonthlyRiskContributionChild = 300,
                MonthlyMsaContributionAdult = 250, // Has MSA
                MonthlyMsaContributionChild = 150,
                // No Principal - correct for Double
                TotalMonthlyContributionsAdult = 750,
                TotalMonthlyContributionsChild = 450
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Double"
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateBulkInsertAsyncShouldFailForDoubleVariantWithPrincipal()
    {
        // Arrange
        var categoryId = 1;
        var bulkInsertDto = new List<CreateMedicalOptionVariantsDto>
        {
            new()
            {
                MedicalOptionName = "Double Plus",
                MedicalOptionCategoryId = categoryId,
                SalaryBracketMin = 0,
                SalaryBracketMax = null,
                MonthlyRiskContributionPrincipal = 1000, // Should not have Principal
                MonthlyRiskContributionAdult = 500,
                MonthlyRiskContributionChild = 300,
                MonthlyMsaContributionAdult = 250,
                MonthlyMsaContributionChild = 150,
                TotalMonthlyContributionsPrincipal = 1000,
                TotalMonthlyContributionsAdult = 750,
                TotalMonthlyContributionsChild = 450
            }
        };

        var categoryInfo = new MedicalOptionCategory
        {
            MedicalOptionCategoryId = categoryId,
            MedicalOptionCategoryName = "Double"
        };

        var existingOptions = new List<MedicalOptionDto>();
        var testDate = new DateTime(2024, 11, 15);

        // Act
        var result = await MedicalOptionValidator.ValidateBulkInsertAsync(
            categoryId, bulkInsertDto, _mockRepository.Object, categoryInfo, existingOptions, testDate);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("should NOT have Principal contributions", result.ErrorMessage);
    }

    #endregion

   }
}