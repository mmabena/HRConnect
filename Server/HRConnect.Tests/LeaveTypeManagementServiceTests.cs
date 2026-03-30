namespace HRConnect.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Models;
    using HRConnect.Api.Services;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    public class LeaveTypeManagementServiceTests
    {
        private static ApplicationDBContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDBContext(options);
        }

        [Fact]
        public async Task GetLeaveTypesShouldReturnAll()
        {
            var context = GetInMemoryDb();

            context.LeaveTypes.AddRange(
    new LeaveType { Id = 1, Name = "Annual", Code = "AL", Description = "Annual Leave", IsActive = true },
    new LeaveType { Id = 2, Name = "Sick", Code = "SL", Description = "Sick Leave", IsActive = true }
);

            await context.SaveChangesAsync();

            var service = new LeaveTypeManagementService(context);

            var result = await service.GetLeaveTypesAsync();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetLeaveTypeByIdShouldReturnCorrectLeaveType()
        {
            var context = GetInMemoryDb();

            context.LeaveTypes.Add(
            new LeaveType { Id = 1, Name = "Annual", Code = "AL", Description = "Annual Leave", IsActive = true });

            await context.SaveChangesAsync();

            var service = new LeaveTypeManagementService(context);

            var result = await service.GetLeaveTypeByIdAsync(1);

            Assert.Equal("Annual", result.Name);
        }

        [Fact]
        public async Task GetLeaveTypeByIdShouldThrowIfNotFound()
        {
            var context = GetInMemoryDb();
            var service = new LeaveTypeManagementService(context);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.GetLeaveTypeByIdAsync(99));
        }

        [Fact]
        public async Task CreateLeaveTypeShouldCreateSuccessfully()
        {
            var context = GetInMemoryDb();

            var service = new LeaveTypeManagementService(context);

            var request = new CreateLeaveTypeRequest
            {
                Name = "Annual",
                Code = "AL",
                Description = "Annual Leave",
                FemaleOnly = false,
                Rules = new List<LeaveEntitlementRuleRequest>
                {
                    new LeaveEntitlementRuleRequest
                    {
                        JobGradeId = 1,
                        MinYearsService = 0,
                        MaxYearsService = null,
                        DaysAllocated = 15
                    }
                }
            };

            var result = await service.CreateLeaveTypeAsync(request);

            Assert.Equal("Annual", result.Name);
            Assert.Single(result.Rules);
        }

        [Fact]
        public async Task CreateLeaveTypeShouldRejectDuplicateName()
        {
            var context = GetInMemoryDb();

            context.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Name = "Annual",
                Code = "AL",
                Description = "Annual Leave",
                IsActive = true
            });

            await context.SaveChangesAsync();

            var service = new LeaveTypeManagementService(context);

            var request = new CreateLeaveTypeRequest
            {
                Name = "Annual",
                Code = "AL2",
                Rules = new List<LeaveEntitlementRuleRequest>
                {
                    new LeaveEntitlementRuleRequest
                    {
                        JobGradeId = 1,
                        MinYearsService = 0,
                        DaysAllocated = 15
                    }
                }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateLeaveTypeAsync(request));
        }

        [Fact]
        public async Task CreateLeaveTypeShouldRejectDuplicateCode()
        {
            var context = GetInMemoryDb();

            context.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Name = "Annual",
                Code = "AL",
                Description = "Annual Leave",
                IsActive = true
            });
            await context.SaveChangesAsync();

            var service = new LeaveTypeManagementService(context);

            var request = new CreateLeaveTypeRequest
            {
                Name = "New Annual",
                Code = "AL",
                Rules = new List<LeaveEntitlementRuleRequest>
                {
                    new LeaveEntitlementRuleRequest
                    {
                        JobGradeId = 1,
                        MinYearsService = 0,
                        DaysAllocated = 15
                    }
                }
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateLeaveTypeAsync(request));
        }

        [Fact]
        public async Task UpdateLeaveTypeShouldUpdateSuccessfully()
        {
            var context = GetInMemoryDb();

            context.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Name = "Annual",
                Code = "AL",
                Description = "Annual Leave",
                IsActive = true
            });

            await context.SaveChangesAsync();

            var service = new LeaveTypeManagementService(context);

            var request = new UpdateLeaveTypeRequest
            {
                Name = "Updated Annual",
                Rules = new List<LeaveEntitlementRuleRequest>
                {
                    new LeaveEntitlementRuleRequest
                    {
                        JobGradeId = 1,
                        MinYearsService = 0,
                        DaysAllocated = 20
                    }
                }
            };

            var result = await service.UpdateLeaveTypeAsync(1, request);

            Assert.Equal("Updated Annual", result.Name);
        }

        [Fact]
        public async Task UpdateLeaveTypeShouldThrowIfNotFound()
        {
            var context = GetInMemoryDb();
            var service = new LeaveTypeManagementService(context);

            var request = new UpdateLeaveTypeRequest
            {
                Name = "Test",
                Rules = new List<LeaveEntitlementRuleRequest>()
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.UpdateLeaveTypeAsync(99, request));
        }

        [Fact]
        public void ValidateRulesShouldRejectNegativeYears()
        {
            var rules = new List<LeaveEntitlementRuleRequest>
            {
                new LeaveEntitlementRuleRequest
                {
                    JobGradeId = 1,
                    MinYearsService = -1,
                    DaysAllocated = 10
                }
            };

            Assert.Throws<InvalidOperationException>(() =>
                LeaveTypeManagementService.ValidateRules(rules));
        }

        [Fact]
        public void ValidateRulesShouldRejectOverlappingRanges()
        {
            var rules = new List<LeaveEntitlementRuleRequest>
            {
                new LeaveEntitlementRuleRequest
                {
                    JobGradeId = 1,
                    MinYearsService = 0,
                    MaxYearsService = 3,
                    DaysAllocated = 10
                },
                new LeaveEntitlementRuleRequest
                {
                    JobGradeId = 1,
                    MinYearsService = 2,
                    MaxYearsService = 5,
                    DaysAllocated = 15
                }
            };

            Assert.Throws<InvalidOperationException>(() =>
                LeaveTypeManagementService.ValidateRules(rules));
        }
    }
}