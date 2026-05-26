namespace HRConnect.Tests
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.CompanyContributions;
  using Microsoft.AspNetCore.DataProtection;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Services;
  using Microsoft.EntityFrameworkCore;
  using Moq;
  using Xunit;

  public class CompanyContributionAllocationServiceTests : IDisposable
  {
    private readonly ApplicationDBContext _context;
    private readonly Mock<IEmployeeCompanyContributionRepository> _repoMock;
    private readonly CompanyContributionAllocationService _service;


    public CompanyContributionAllocationServiceTests()
    {
      var options = new DbContextOptionsBuilder<ApplicationDBContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString())
          .Options;
      // Create a mock IDataProtectionProvider
      var mockProvider = new Mock<IDataProtectionProvider>();
      // Setup CreateProtector to return a dummy protector
      var mockProtector = new Mock<IDataProtector>();
      mockProtector.Setup(p => p.Protect(It.IsAny<byte[]>())).Returns<byte[]>(b => b);
      mockProtector.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Returns<byte[]>(b => b);
      mockProvider.Setup(p => p.CreateProtector(It.IsAny<string>())).Returns(mockProtector.Object);
      _context = new ApplicationDBContext(options, mockProtector.Object);

      _repoMock = new Mock<IEmployeeCompanyContributionRepository>();

      _repoMock
          .Setup(r => r.AddRangeAsync(It.IsAny<List<EmployeeCompanyContribution>>()))
          .Returns(Task.CompletedTask)
          .Callback<List<EmployeeCompanyContribution>>(records =>
          {
            _context.EmployeeCompanyContributions.AddRange(records);
            _context.SaveChanges();
          });

      _service = new CompanyContributionAllocationService(
          _context,
          _repoMock.Object
      );

      SeedData();
    }

    private void SeedData()
    {
      _context.PayrollRuns.Add(new PayrollRun
      {
        PayrollRunId = 1,
        PayrollRunNumber = 1,
        IsLocked = false
      });

      _context.CompanyContributions.AddRange(
          new CompanyContribution
          {
            Code = "DEATHBEN",
            Percentage = 0.00565m,
            IsActive = true
          },
          new CompanyContribution
          {
            Code = "DISABILITY",
            Percentage = 0.00482m,
            IsActive = true
          }
      );

      _context.SaveChanges();
    }

    [Fact]
    public async Task AllocateAsync_EmployeeOver65_ShouldBeExcluded()
    {
      _context.Employees.Add(new Employee
      {
        EmployeeId = "EMP001",
        Name = "Old",
        Surname = "Guy",
        EmploymentStatus = EmploymentStatus.Permanent,
        MonthlySalary = 10000,
        DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-66))
      });

      _context.SaveChanges();

      await _service.AllocateAsync(1);

      var records = _context.EmployeeCompanyContributions.ToList();

      Assert.Empty(records);
    }

    [Fact]
    public async Task AllocateAsync_FixedTermEmployee_ShouldBeExcluded()
    {
      _context.Employees.Add(new Employee
      {
        EmployeeId = "EMP002",
        Name = "Temp",
        Surname = "Worker",
        EmploymentStatus = EmploymentStatus.FixedTerm,
        MonthlySalary = 10000,
        DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-30))
      });

      _context.SaveChanges();

      await _service.AllocateAsync(1);

      var records = _context.EmployeeCompanyContributions.ToList();

      Assert.Empty(records);
    }

    [Fact]
    public async Task AllocateAsync_FixedTermPromotedToPermanent_ShouldBeIncludedNextRun()
    {
      var employee = new Employee
      {
        EmployeeId = "EMP003",
        Name = "Promoted",
        Surname = "User",
        EmploymentStatus = EmploymentStatus.FixedTerm,
        MonthlySalary = 10000,
        DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-30))
      };

      _context.Employees.Add(employee);
      _context.SaveChanges();

      // First run → should NOT include
      await _service.AllocateAsync(1);

      Assert.Empty(_context.EmployeeCompanyContributions);

      // Promote employee
      employee.EmploymentStatus = EmploymentStatus.Permanent;
      _context.SaveChanges();

      // New payroll run
      _context.PayrollRuns.Add(new PayrollRun
      {
        PayrollRunId = 2,
        PayrollRunNumber = 2,
        IsLocked = false
      });
      _context.SaveChanges();

      await _service.AllocateAsync(2);

      var records = _context.EmployeeCompanyContributions
          .Where(r => r.PayrollRunId == 2)
          .ToList();

      Assert.Single(records);
      Assert.Equal("EMP003", records.First().EmployeeId);
    }

    [Fact]
    public async Task AllocateAsync_ShouldCalculateCorrectPercentagesAndAmounts()
    {
      _context.Employees.Add(new Employee
      {
        EmployeeId = "EMP004",
        Name = "Jeff",
        Surname = "Test",
        EmploymentStatus = EmploymentStatus.Permanent,
        MonthlySalary = 10000,
        DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddYears(-30))
      });

      _context.SaveChanges();

      await _service.AllocateAsync(1);

      var record = _context.EmployeeCompanyContributions.First();

      Assert.Equal(0.00565m, record.DeathPercentage);
      Assert.Equal(56.5m, record.DeathAmount);

      Assert.Equal(0.00482m, record.DisabilityPercentage);
      Assert.Equal(48.2m, record.DisabilityAmount);
    }

    public void Dispose()
    {
      _context.Dispose();
      GC.SuppressFinalize(this);
    }
  }
}