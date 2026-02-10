namespace HRConnect.Tests.Services
{
    using HRConnect.Api.Data;
    using HRConnect.Api.Models;
    using HRConnect.Api.Repository;
    using HRConnect.Api.Services;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Reflection.Metadata;
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
                new LeaveType { LeaveTypeId = 2, Name = "Sick Leave", Code = "SL", DaysEntitled = 0 },
                new LeaveType { LeaveTypeId = 3, Name = "Family Responsibility Leave", Code = "FRL", DaysEntitled = 0 }
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
                new LeaveType { LeaveTypeId = 2, Name = "Sick Leave", Code = "SL", DaysEntitled = 0 },
                new LeaveType { LeaveTypeId = 3, Name = "Family Responsibility Leave", Code = "FRL", DaysEntitled = 0 }
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
                new LeaveType { LeaveTypeId = 2, Name = "Sick Leave", Code = "SL", DaysEntitled = 0 },
                new LeaveType { LeaveTypeId = 3, Name = "Family Responsibility Leave", Code = "FRL", DaysEntitled = 0 }
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
        [Fact]
        public async Task AllocateOnEmployeeHireAssignsFamilyResponsibilityLeave()
        {
            using var context = CreateDbContext();
            //Arrange: Employee
            var employee = new Employee
            {
                EmployeeId = 1,
                Name = "Mpho",
                Surname = "Mosia",
                Gender = "Male",
                ReportingManager = "Manager",
                JobGrade = "Junior Management",
                DateCreated = DateTime.UtcNow.AddYears(-1)
            };
            context.Employees.Add(employee);

            //Arrange: JobGrade
            context.JobGrades.Add(new JobGrade
            {
                JobGradeId = 1,
                EmployeeId = 1,
                EmployeeName = "Mpho Mosia",
                ReportingManager = "Manager",
                JobGradeName = "Junior Management",
                CreatedDate = employee.DateCreated
            });

            //Arrange: Leave Types
            context.LeaveTypes.AddRange(
                new LeaveType { LeaveTypeId = 1, Name = "Annual Leave", Code = "AL", DaysEntitled = 0 },
                new LeaveType { LeaveTypeId = 2, Name = "Sick Leave", Code = "SL", DaysEntitled = 0 },
                new LeaveType { LeaveTypeId = 3, Name = "Family Responsibility Leave", Code = "FRL", DaysEntitled = 0 }
            );
            await context.SaveChangesAsync();

            var service = new LeaveEntitlementService(
                new EmployeeRepository(context),
                new JobGradeRepository(context),
                new LeaveTypeRepository(context),
                new LeaveEntitlementRepository(context),
                new EmployeeLeaveBalanceRepository(context)
            );

            //Act
            await service.AllocateOnEmployeeHireAsync(1);

            //Assert: Family Responsibility Leave Entitlement
            var frlEntitlement = await context.LeaveEntitlementRules
            .FirstAsync(e =>
            context.LeaveTypes.Any(t =>
            t.LeaveTypeId == e.LeaveTypeId &&
            t.Code == "FRL"));

            var frlBalance = await context.EmployeeLeaveBalances
            .FirstAsync(b =>
            context.LeaveTypes.Any(t =>
            t.LeaveTypeId == b.LeaveTypeId &&
            t.Code == "FRL"));

            Assert.Equal(3, frlEntitlement.DaysEntitled);
            Assert.Equal(3, frlBalance.DaysAvailable);
        }
        [Fact]
        public async Task AllocateOnEmployeeHireFemaleEmployeeGetsMaternityLeave()
        {
            using var context = CreateDbContext();

            // Arrange: Female employee
            var employee = new Employee
            {
                EmployeeId = 1,
                Name = "Jane",
                Surname = "Doe",
                Gender = "Female",
                ReportingManager = "Manager",
                JobGrade = "Junior Management",
                DateCreated = DateTime.UtcNow.AddMonths(-2)
            };

            context.Employees.Add(employee);

            context.JobGrades.Add(new JobGrade
            {
                JobGradeId = 1,
                EmployeeId = 1,
                EmployeeName = "Jane Doe",
                ReportingManager = "Manager",
                JobGradeName = "Junior Management",
                CreatedDate = employee.DateCreated
            });

            // Arrange: Leave Types (AL, SL, FRL, ML)
            context.LeaveTypes.AddRange(
                new LeaveType { LeaveTypeId = 1, Name = "Annual Leave", Code = "AL", DaysEntitled = 0 },
                new LeaveType { LeaveTypeId = 2, Name = "Sick Leave", Code = "SL", DaysEntitled = 0 },
                new LeaveType { LeaveTypeId = 3, Name = "Family Responsibility Leave", Code = "FRL", DaysEntitled = 0 },
                new LeaveType { LeaveTypeId = 4, Name = "Maternity Leave", Code = "ML", DaysEntitled = 0 }
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

            // Assert: Maternity Leave entitlement exists
            var maternityEntitlement = await context.LeaveEntitlementRules
                .FirstOrDefaultAsync(e =>
                    context.LeaveTypes.Any(t =>
                        t.LeaveTypeId == e.LeaveTypeId &&
                        t.Code == "ML"));

            var maternityBalance = await context.EmployeeLeaveBalances
                .FirstOrDefaultAsync(b =>
                    context.LeaveTypes.Any(t =>
                        t.LeaveTypeId == b.LeaveTypeId &&
                        t.Code == "ML"));

            Assert.NotNull(maternityEntitlement);
            Assert.NotNull(maternityBalance);
            Assert.Equal(120, maternityEntitlement!.DaysEntitled);
            Assert.Equal(120, maternityBalance!.DaysAvailable);
        }
        [Fact]
        public async Task AllocateOnEmployeeHireMaleEmployeeDoesNotGetMaternityLeave()
        {
            using var context = CreateDbContext();

            // Arrange: Male employee
            var employee = new Employee
            {
                EmployeeId = 1,
                Name = "John",
                Surname = "Doe",
                Gender = "Male",
                ReportingManager = "Manager",
                JobGrade = "Junior Management",
                DateCreated = DateTime.UtcNow.AddMonths(-2)
            };

            context.Employees.Add(employee);

            context.JobGrades.Add(new JobGrade
            {
                JobGradeId = 1,
                EmployeeId = 1,
                EmployeeName = "John Doe",
                ReportingManager = "Manager",
                JobGradeName = "Junior Management",
                CreatedDate = employee.DateCreated
            });

            // Arrange: Leave Types (AL, SL, FRL, ML)
            context.LeaveTypes.AddRange(
                new LeaveType { LeaveTypeId = 1, Name = "Annual Leave", Code = "AL", DaysEntitled = 0 },
                new LeaveType { LeaveTypeId = 2, Name = "Sick Leave", Code = "SL", DaysEntitled = 0 },
                new LeaveType { LeaveTypeId = 3, Name = "Family Responsibility Leave", Code = "FRL", DaysEntitled = 0 },
                new LeaveType { LeaveTypeId = 4, Name = "Maternity Leave", Code = "ML", DaysEntitled = 0 }
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

            // Assert: No maternity leave records created
            var maternityEntitlementExists = await context.LeaveEntitlementRules
                .AnyAsync(e =>
                    context.LeaveTypes.Any(t =>
                        t.LeaveTypeId == e.LeaveTypeId &&
                        t.Code == "ML"));

            var maternityBalanceExists = await context.EmployeeLeaveBalances
                .AnyAsync(b =>
                    context.LeaveTypes.Any(t =>
                        t.LeaveTypeId == b.LeaveTypeId &&
                        t.Code == "ML"));

            Assert.False(maternityEntitlementExists);
            Assert.False(maternityBalanceExists);
        }

    }
}
