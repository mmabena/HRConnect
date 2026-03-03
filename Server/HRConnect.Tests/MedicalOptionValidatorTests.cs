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
         var result = await MedicalOptionValidator.ValidateAllIdsExistAsync(bulkUpdateDto, _mockRepository.Object, dbData);
         
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
         var result = await MedicalOptionValidator.ValidateAllIdsExistAsync(bulkUpdateDto, _mockRepository.Object, dbData);
         
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
         var result = await MedicalOptionValidator.ValidateAllIdsInCategoryAsync(bulkUpdateDto, categoryId, _mockRepository.Object, dbData);
         
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
         var result = await MedicalOptionValidator.ValidateAllIdsInCategoryAsync(bulkUpdateDto, categoryId, _mockRepository.Object, dbData);
         
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
   }
}