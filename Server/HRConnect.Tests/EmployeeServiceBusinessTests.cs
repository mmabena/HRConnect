namespace HRConnect.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using HRConnect.Api.Services;
    using Microsoft.EntityFrameworkCore;
    using Xunit;
    using HRConnect.Api.DTOs.Employee;
    using Moq;
    using HRConnect.Api.Utils;
    using System.Threading;
    using Microsoft.EntityFrameworkCore.Storage;

    public class EmployeeServiceBusinessTests
    {
        private sealed class FakeEmailService : IEmailService
        {
            public int EmailsSent { get; private set; }

            public Task SendEmailAsync(string recipientEmail, string subject, string body)
            {
                EmailsSent++;
                return Task.CompletedTask;
            }
        }

        private static ApplicationDBContext GetDb()
        {
            return new ApplicationDBContext(
                new DbContextOptionsBuilder<ApplicationDBContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options);
        }

        private static LeaveBalanceService GetBalanceService(ApplicationDBContext db)
            => new LeaveBalanceService(db);

        private static LeaveProcessingService GetProcessingService(ApplicationDBContext db)
            => new LeaveProcessingService(db, new FakeEmailService(), GetBalanceService(db));

        private static EmployeeService GetService(ApplicationDBContext db, FakeEmailService email)
        {
            var employeeRepoMock = new Mock<IEmployeeRepository>();
            var positionRepoMock = new Mock<IPositionRepository>();
            var transactionMock = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();

            transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            employeeRepoMock.Setup(x => x.BeginTransactionAsync())
                .ReturnsAsync(transactionMock.Object);

            // 🔥 FIX 1: RETURN DATA FROM DB
            employeeRepoMock.Setup(x => x.GetEmployeeByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) => db.Employees.FirstOrDefault(e => e.EmployeeId == id));

            employeeRepoMock.Setup(x => x.UpdateEmployeeAsync(It.IsAny<Employee>()))
                .ReturnsAsync((Employee e) => e);

            employeeRepoMock.Setup(x => x.CreateEmployeeAsync(It.IsAny<Employee>()))
            .ReturnsAsync((Employee e) =>
            {
                db.Employees.Add(e);  
                db.SaveChanges();
                return e;
            });

            employeeRepoMock.Setup(x => x.GetAllEmployeeIdsWithPrefix(It.IsAny<string>()))
                .ReturnsAsync(new List<string>());

            // 🔥 FIX 2: DUPLICATE VALIDATION CALLS
            employeeRepoMock.Setup(x => x.GetEmployeeByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((Employee?)null);

            employeeRepoMock.Setup(x => x.GetEmployeeByTaxNumberAsync(It.IsAny<string>()))
                .ReturnsAsync((Employee?)null);

            employeeRepoMock.Setup(x => x.GetEmployeeByIdNumberAsync(It.IsAny<string>()))
                .ReturnsAsync((Employee?)null);

            employeeRepoMock.Setup(x => x.GetEmployeeByContactNumberAsync(It.IsAny<string>()))
                .ReturnsAsync((Employee?)null);

            positionRepoMock.Setup(x => x.GetPositionByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => db.Positions.FirstOrDefault(p => p.PositionId == id));

            return new EmployeeService(
                db,
                employeeRepoMock.Object,
                email,
                positionRepoMock.Object,
                GetBalanceService(db),
                GetProcessingService(db)
            );
        }

        // ================= CREATE =================

        [Fact]
        public async Task CreateEmployee_ShouldInitializeLeaveBalances()
        {
            var db = GetDb();
            var email = new FakeEmailService();
            var service = GetService(db, email);

            db.JobGrades.Add(new JobGrade { JobGradeId = 1, Name = "G1" });
            db.OccupationalLevels.Add(new OccupationalLevel { OccupationalLevelId = 1, Description = "Level 1" });
            db.Positions.Add(new Position { PositionId = 1, JobGradeId = 1, OccupationalLevelId = 1 });

            db.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Code = "AL",
                Name = "Annual Leave",
                Description = "Annual Leave",
                IsActive = true
            });

            db.LeaveEntitlementRules.Add(new LeaveEntitlementRule
            {
                Id = 1,
                LeaveTypeId = 1,
                JobGradeId = 1,
                DaysAllocated = 15,
                MinYearsService = 0,
                IsActive = true
            });

            await db.SaveChangesAsync();

            var result = await service.CreateEmployeeAsync(new CreateEmployeeRequestDto
            {
                Name = "Test",
                Surname = "User",
                Email = "test@singular.co.za",
                Title = Title.Mr,
                Gender = Gender.Male,
                ContactNumber = "0123456789",
                PhysicalAddress = "Address",
                TaxNumber = "1234567890",
                IdNumber = "0309195036087",
                Nationality = "South African",
                Branch = Branch.Johannesburg,
                City = "Johannesburg",
                ZipCode = "2000",
                PositionId = 1,
                MonthlySalary = 10000,
                EmploymentStatus = EmploymentStatus.Permanent,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                ProfileImage = "img.jpg"
            });

            Assert.Single(db.EmployeeLeaveBalances);
        }

        // ================= UPDATE =================

        [Fact]
        public async Task UpdatePosition_ShouldCreateNewAccrualSegment()
        {
            var db = GetDb();
            var email = new FakeEmailService();
            var service = GetService(db, email);

            db.JobGrades.AddRange(
                new JobGrade { JobGradeId = 1, Name = "G1" },
                new JobGrade { JobGradeId = 2, Name = "G2" });

            db.OccupationalLevels.Add(new OccupationalLevel { OccupationalLevelId = 1, Description = "Level 1" });

            db.Positions.AddRange(
                new Position { PositionId = 1, JobGradeId = 1, OccupationalLevelId = 1 },
                new Position { PositionId = 2, JobGradeId = 2, OccupationalLevelId = 1 });

            db.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Code = "AL",
                Name = "Annual Leave",
                Description = "Annual Leave",
                IsActive = true
            });

            db.LeaveEntitlementRules.AddRange(
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 1, DaysAllocated = 15, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 1, JobGradeId = 2, DaysAllocated = 20, IsActive = true });

            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid().ToString(),
                PositionId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
                Email = "test@singular.co.za",
                Gender = Gender.Male,
                Name = "Test",
                Surname = "User",
                ContactNumber = "0123456789",
                Nationality = "South African"
            };

            db.Employees.Add(employee);
            await db.SaveChangesAsync();

            await GetBalanceService(db).InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            await service.UpdateEmployeeAsync(employee.EmployeeId, new UpdateEmployeeRequestDto
            {
                Title = Title.Mr,
                Gender = Gender.Male,
                Name = "Test",
                Surname = "User",
                IdNumber = "0305054589589",
                Nationality = "South African",
                Email = "test@singular.co.za",
                ContactNumber = "0123456789",
                City = "Johannesburg",
                ZipCode = "2000",
                Branch = Branch.Johannesburg,
                MonthlySalary = 10000,
                PositionId = 2,
                EmploymentStatus = EmploymentStatus.Permanent,
                ProfileImage = "img.jpg"
            });

            Assert.Equal(2, db.EmployeeAccrualRateHistories.Count());
        }

        // ================= VALIDATION =================

        [Fact]
        public async Task UpdatePosition_ShouldThrowIfEmployeeNotFound()
        {
            var db = GetDb();
            var service = GetService(db, new FakeEmailService());

            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.UpdateEmployeeAsync("invalid", new UpdateEmployeeRequestDto
                {
                    Title = Title.Mr,
                    Gender = Gender.Male,
                    Name = "Test",
                    Surname = "User",
                    IdNumber = "0305054589589",
                    Nationality = "South African",
                    Email = "test@singular.co.za",
                    ContactNumber = "0123456789",
                    City = "Johannesburg",
                    ZipCode = "2000",
                    Branch = Branch.Johannesburg,
                    MonthlySalary = 10000,
                    PositionId = 1,
                    EmploymentStatus = EmploymentStatus.Permanent,
                    ProfileImage = "img.jpg"
                }));
        }
    }
}