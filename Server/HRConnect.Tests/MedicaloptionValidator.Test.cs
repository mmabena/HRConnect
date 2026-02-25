using Xunit;
using Moq;
using Moq.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using HRConnect.Api.Utils.MedicalOptions;
using HRConnect.Api.DTOs.MedicalOption;
using HRConnect.Api.Models;
using HRConnect.Api.Interfaces;
using HRConnect.Api.Models.MedicalOptions.Records;

namespace HRConnect.Tests
{
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
            var result = MedicalOptionValidator.ValidateUpdatePeriod();
            
            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateUpdatePeriodShouldReturnFalseOutsideNovemberDecember()
        {
            // Arrange
            var testDate = new DateTime(2024, 10, 15); // October
            
            // Act
            var result = MedicalOptionValidator.ValidateUpdatePeriod();
            
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
                MonthlyRiskContributionPrincipal = 1000,
                MonthlyMsaContributionPrincipal = 500
            };
            
            var dbOption = new MedicalOption
            {
                MonthlyRiskContributionPrincipal = 1000,
                MonthlyMsaContributionPrincipal = 500
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

        // Integration Tests with Mock Repository
        [Fact]
        public async Task ValidateAllCategoryVariantsComprehensiveAsyncShouldPassValidPayload()
        {
            // Arrange
            var categoryId = 1;
            var bulkUpdateDto = new List<UpdateMedicalOptionVariantsDto>
            {
                new() { MedicalOptionId = 1, MonthlyRiskContributionPrincipal = 1000 },
                new() { MedicalOptionId = 2, MonthlyRiskContributionPrincipal = 1500 }
            };
            
            var dbData = new List<MedicalOption>
            {
                new() { MedicalOptionId = 1, MedicalOptionName = "Alliance Plus", MedicalOptionCategoryId = 1 },
                new() { MedicalOptionId = 2, MedicalOptionName = "Alliance Network", MedicalOptionCategoryId = 1 }
            };
            
            _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
                       .ReturnsAsync(dbData);
            
            // Act
            var result = await MedicalOptionValidator.ValidateAllCategoryVariantsComprehensiveAsync(
                categoryId, bulkUpdateDto, _mockRepository.Object, dbData);
            
            // Assert
            Assert.True(result.IsValid);
            _mockRepository.Verify(r => r.GetAllOptionsUnderCategoryAsync(categoryId), Times.Once);
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
            
            _mockRepository.Setup(r => r.GetAllOptionsUnderCategoryAsync(categoryId))
                       .ReturnsAsync(dbData);
            
            // Act
            var result = await MedicalOptionValidator.ValidateAllCategoryVariantsComprehensiveAsync(
                categoryId, bulkUpdateDto, _mockRepository.Object, dbData);
            
            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("do not exist", result.ErrorMessage);
            _mockRepository.Verify(r => r.GetAllOptionsUnderCategoryAsync(categoryId), Times.Once);
        }
    }
}