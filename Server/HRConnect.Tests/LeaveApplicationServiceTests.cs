namespace HRConnect.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Models;
    using HRConnect.Api.Services;
    using HRConnect.Api.Interfaces;
    using Microsoft.EntityFrameworkCore;
    using HRConnect.Api.Utils;
    using Xunit;

    public class LeaveApplicationServiceTests
    {
        private sealed class TrackingEmailService : IEmailService
        {
            public int EmailsSent { get; private set; }

            public Task SendEmailAsync(string recipientEmail, string subject, string body)
            {
                EmailsSent++;
                return Task.CompletedTask;
            }
        }

        private static ApplicationDBContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDBContext(options);
        }

        private static async Task<(ApplicationDBContext, Employee)> SetupEmployee()
        {
            var context = GetInMemoryDb();

            // Create manager
            var manager = new Employee
            {
                EmployeeId = Guid.NewGuid().ToString(),
                Name = "Manager",
                Surname = "User",
                Email = "manager@email.com",
                Gender = Gender.Male,
                PositionId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-5))
            };

            context.Employees.Add(manager);

            // Create employee linked to manager
            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid().ToString(),
                Name = "Test",
                Surname = "User",
                Email = "test@email.com",
                Gender = Gender.Male,
                CareerManagerID = "manager@email.com",
                PositionId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1))
            };

            context.Employees.Add(employee);

            context.LeaveTypes.Add(new LeaveType
            {
                Id = 1,
                Name = "Annual",
                Code = "AL",
                Description = "Annual Leave",
                IsActive = true
            });

            context.EmployeeLeaveBalances.Add(new EmployeeLeaveBalance
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = 1,
                AccruedDays = 10,
                AvailableDays = 10,
                TakenDays = 0
            });

            await context.SaveChangesAsync();

            return (context, employee);
        }
        [Fact]
        public async Task ApplyForLeaveShouldCreateApplication()
        {
            var (context, employee) = await SetupEmployee();
            var email = new TrackingEmailService();

            var service = new LeaveApplicationService(context, email);

            var request = new CreateApplicationRequest
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(6)),
                Description = "Vacation"
            };

            var result = await service.ApplyForLeaveAsync(request);

            Assert.Equal("Pending", result.Status);
            Assert.Equal(1, context.LeaveApplications.Count());
        }

        [Fact]
        public async Task ApplyForLeaveShouldFailIfEmployeeNotFound()
        {
            var context = GetInMemoryDb();
            var email = new TrackingEmailService();
            var service = new LeaveApplicationService(context, email);

            var request = new CreateApplicationRequest
            {
                EmployeeId = Guid.NewGuid().ToString(),
                LeaveTypeId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ApplyForLeaveAsync(request));
        }

        [Fact]
        public async Task ApplyForLeaveShouldRejectInvalidDateRange()
        {
            var (context, employee) = await SetupEmployee();
            var service = new LeaveApplicationService(context, new TrackingEmailService());

            var request = new CreateApplicationRequest
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ApplyForLeaveAsync(request));
        }

        [Fact]
        public async Task ApplyForLeaveShouldRejectCrossYearRequests()
        {
            var (context, employee) = await SetupEmployee();
            var service = new LeaveApplicationService(context, new TrackingEmailService());

            var request = new CreateApplicationRequest
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = 1,
                StartDate = new DateOnly(DateTime.UtcNow.Year, 12, 31),
                EndDate = new DateOnly(DateTime.UtcNow.Year + 1, 1, 2)
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ApplyForLeaveAsync(request));
        }

        [Fact]
        public async Task ApplyForLeaveShouldRejectInsufficientBalance()
        {
            var (context, employee) = await SetupEmployee();
            var service = new LeaveApplicationService(context, new TrackingEmailService());

            var balance = context.EmployeeLeaveBalances.First();
            balance.AvailableDays = 0;
            await context.SaveChangesAsync();

            var request = new CreateApplicationRequest
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10))
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ApplyForLeaveAsync(request));
        }

        [Fact]
        public async Task ApproveLeaveShouldUpdateBalance()
        {
            var (context, employee) = await SetupEmployee();
            var service = new LeaveApplicationService(context, new TrackingEmailService());

            var application = new LeaveApplication
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(6)),
                DaysRequested = 2,
                Description = "Vacation",
                Status = LeaveApplication.LeaveApplicationStatus.Pending,
                ApprovalToken = Guid.NewGuid(),
                TokenExpiry = DateTime.UtcNow.AddHours(2)
            };

            context.LeaveApplications.Add(application);
            await context.SaveChangesAsync();

            await service.ApproveLeaveAsync(application.Id, application.ApprovalToken);

            var balance = context.EmployeeLeaveBalances.First();

            Assert.Equal(2, balance.TakenDays);
            Assert.Equal(8, balance.AvailableDays);
        }

        [Fact]
        public async Task RejectLeaveShouldUpdateStatus()
        {
            var (context, employee) = await SetupEmployee();
            var service = new LeaveApplicationService(context, new TrackingEmailService());

            var application = new LeaveApplication
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(6)),
                DaysRequested = 2,
                Description = "Vacation",
                Status = LeaveApplication.LeaveApplicationStatus.Pending,
                ApprovalToken = Guid.NewGuid(),
                TokenExpiry = DateTime.UtcNow.AddHours(2)
            };

            context.LeaveApplications.Add(application);
            await context.SaveChangesAsync();

            await service.RejectLeaveAsync(application.Id, application.ApprovalToken, "Not approved");

            var updated = context.LeaveApplications.First();

            Assert.Equal("Rejected", updated.Status.ToString());
            Assert.Equal("Not approved", updated.RejectionReason);
        }

        [Fact]
        public async Task ApproveShouldFailWithInvalidToken()
        {
            var (context, employee) = await SetupEmployee();
            var service = new LeaveApplicationService(context, new TrackingEmailService());

            var application = new LeaveApplication
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(6)),
                DaysRequested = 2,
                Description = "Vacation",
                Status = LeaveApplication.LeaveApplicationStatus.Pending,
                ApprovalToken = Guid.NewGuid(),
                TokenExpiry = DateTime.UtcNow.AddHours(2)
            };

            context.LeaveApplications.Add(application);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ApproveLeaveAsync(application.Id, Guid.NewGuid()));
        }

        [Fact]
        public async Task ApproveShouldFailIfTokenExpired()
        {
            var (context, employee) = await SetupEmployee();
            var service = new LeaveApplicationService(context, new TrackingEmailService());

            var token = Guid.NewGuid();

            var application = new LeaveApplication
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(6)),
                DaysRequested = 2,
                Description = "Vacation",
                Status = LeaveApplication.LeaveApplicationStatus.Pending,
                ApprovalToken = token,
                TokenExpiry = DateTime.UtcNow.AddMinutes(-5)
            };

            context.LeaveApplications.Add(application);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ApproveLeaveAsync(application.Id, token));
        }
    }
}