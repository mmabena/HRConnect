namespace HRConnect.Tests.Services
{
    using HRConnect.Api.Data;
    using HRConnect.Api.Models;
    using HRConnect.Api.Repository;
    using HRConnect.Api.Services;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Reflection.Metadata.Ecma335;
    using System.Threading.Tasks;
    using Xunit;

    public class LeaveEntitlementServiceTests
    {
        private static ApplicationDBContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDBContext(options);
        }

        [Theory]
        [InlineData("Junior Management", 2, 15)]
        [InlineData("Middle Management", 2, 15)]
        [InlineData("Skilled/Semi Skilled", 2, 15)]
        [InlineData("Unskilled", 2, 15)]
        [InlineData("Top/Senior Management", 2, 18)]
        [InlineData("Executive Director", 2, 22)]
        public async Task AllocateOnEmployeeHireLessThan3YearsAssignsCorrectAnnualLeave(
            string jobGradeName,
            int yearsEmployed,
            int expectedDays)
        {
            using var context = CreateDbContext();

            // Arrange: Employee
            var employee = new Employee
            {
                EmployeeId = 1,
                Name = "Mpho",
                Surname = "Mosia",
                Gender = "Male",
                ReportingManager = "Manager",
                JobGrade = jobGradeName,
                DateCreated = DateTime.UtcNow.AddYears(-yearsEmployed)
            };

            context.Employees.Add(employee);

            // Arrange: Job Grade
            context.JobGrades.Add(new JobGrade
            {
                JobGradeId = 1,
                EmployeeId = 1,
                EmployeeName = "Mpho Mosia",
                ReportingManager = "Manager",
                JobGradeName = jobGradeName,
                CreatedDate = employee.DateCreated
            });

            // Arrange: Leave Types (Annual + Sick)
            context.LeaveTypes.AddRange(
                new LeaveType { LeaveTypeId = 1, Name = "Annual Leave", Code = "AL", DaysEntitled = 0 },
                new LeaveType { LeaveTypeId = 2, Name = "Sick Leave", Code = "SL", DaysEntitled = 0 }
            );

            await context.SaveChangesAsync();

            var service = new LeaveEntitlementService(
                new EmployeeRepository(context),
                new JobGradeRepository(context),
                new LeaveTypeRepository(context),
                new LeaveEntitlementRepository(context),
                new EmployeeLeaveBalanceRepository(context)
            );

            // Act
            await service.AllocateOnEmployeeHireAsync(1);

            // Assert - pick the annual leave records explicitly
            var annualType = await context.LeaveTypes.FirstAsync(l => l.Code == "AL");
            var entitlement = await context.LeaveEntitlementRules.FirstAsync(e => e.LeaveTypeId == annualType.LeaveTypeId);
            var balance = await context.EmployeeLeaveBalances.FirstAsync(b => b.LeaveTypeId == annualType.LeaveTypeId);

            Assert.Equal(expectedDays, entitlement.DaysEntitled);
            Assert.Equal(expectedDays, balance.DaysAvailable);
        }
        [Fact]
        public async Task AllocateOnEmployeeHireThreeMonthsEmployedSickLeaveBalanceIsThree()
        {
            using var context = CreateDbContext();

            var employee = new Employee
            {
                EmployeeId = 1,
                Name = "Mpho",
                Surname = "Mosia",
                Gender = "Male",
                ReportingManager = "Manager",
                JobGrade = "Junior Management",
                DateCreated = DateTime.UtcNow.AddMonths(-3)
            };
            context.Employees.Add(employee);

            context.JobGrades.Add(new JobGrade
            {
                JobGradeId = 1,
                EmployeeId = 1,
                EmployeeName = "Mpho Mosia",
                ReportingManager = "Manager",
                JobGradeName = "Junior Management",
                CreatedDate = employee.DateCreated
            });
            context.LeaveTypes.AddRange(
                new LeaveType { LeaveTypeId = 1, Name = "Annual Leave", Code = "AL", DaysEntitled = 0 },
                new LeaveType { LeaveTypeId = 2, Name = "Sick Leave", Code = "SL", DaysEntitled = 0 }
            );
            await context.SaveChangesAsync();

            var service = new LeaveEntitlementService(
                new EmployeeRepository(context),
                new JobGradeRepository(context),
                new LeaveTypeRepository(context),
                new LeaveEntitlementRepository(context),
                new EmployeeLeaveBalanceRepository(context)
            );
            await service.AllocateOnEmployeeHireAsync(1);

            var sickBalance = await context.EmployeeLeaveBalances.FirstAsync(b => b.LeaveTypeId == 2);
            var sickEntitlement = await context.LeaveEntitlementRules.FirstAsync(e => e.LeaveTypeId == 2);

            Assert.Equal(3, sickBalance.DaysAvailable);
            Assert.Equal(30, sickEntitlement.DaysEntitled);
        }
        [Fact]
        public async Task AllocateOnEmployeeHireEightMonthsEmployedSickLeaveBalanceIsThirty()
        {
            using var context = CreateDbContext();

            var employee = new Employee
            {
                EmployeeId = 1,
                Name = "Test",
                Surname = "User",
                Gender = "Male",
                ReportingManager = "Manager",
                JobGrade = "Junior Management",
                DateCreated = DateTime.UtcNow.AddMonths(-8)
            };

            context.Employees.Add(employee);

            context.JobGrades.Add(new JobGrade
            {
                JobGradeId = 1,
                EmployeeId = 1,
                EmployeeName = "Test User",
                ReportingManager = "Manager",
                JobGradeName = "Junior Management",
                CreatedDate = employee.DateCreated
            });

            context.LeaveTypes.AddRange(
                new LeaveType { LeaveTypeId = 1, Name = "Annual Leave", Code = "AL", DaysEntitled = 0 },
                new LeaveType { LeaveTypeId = 2, Name = "Sick Leave", Code = "SL", DaysEntitled = 0 }
            );

            await context.SaveChangesAsync();

            var service = new LeaveEntitlementService(
                new EmployeeRepository(context),
                new JobGradeRepository(context),
                new LeaveTypeRepository(context),
                new LeaveEntitlementRepository(context),
                new EmployeeLeaveBalanceRepository(context));

            await service.AllocateOnEmployeeHireAsync(1);

            var sickBalance = await context.EmployeeLeaveBalances
                .FirstAsync(b => b.LeaveTypeId == 2);

            var sickEntitlement = await context.LeaveEntitlementRules
                .FirstAsync(e => e.LeaveTypeId == 2);

            Assert.Equal(30, sickBalance.DaysAvailable);
            Assert.Equal(30, sickEntitlement.DaysEntitled);
        }

    }
}
