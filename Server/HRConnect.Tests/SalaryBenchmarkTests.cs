using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using HRConnect.Api.DTOs.Benchmarking;
using HRConnect.Api.Models.Benchmarking;
using HRConnect.Api.Models;
using HRConnect.Api.Repository;
using HRConnect.Api.Services;
using Moq;

namespace HRConnect.Tests
{
    public class SalaryBenchmarkTests
    {
        private readonly Mock<ISalaryBenchmarkRepository> _mockRepository;
        private readonly SalaryBenchmarkService _service;

        public SalaryBenchmarkTests()
        {
            _mockRepository = new Mock<ISalaryBenchmarkRepository>();
            _service = new SalaryBenchmarkService(_mockRepository.Object);
        }

        [Fact]
        public async Task CreateAsync_WhenCalledWithValidRequest_ReturnsMappedResponse()
        {
            var request = new SalaryBenchmarkRequestDto
            {
                PositionId = 1,
                Location = "Johannesburg",
                Salary25th = 30000,
                Salary50th = 45000,
                Salary75th = 60000,
                Source = "Glassdoor 2025"
            };

            var fakeCreatedBenchmark = new SalaryBenchmark
            {
                Id = 1,
                PositionId = 1,
                Position = new Position
                {
                    PositionId = 1,
                    PositionTitle = "Software Engineer",
                    JobGrade = new JobGrade { Name = "Middle Management" }
                },
                Location = "Johannesburg",
                Salary25th = 30000,
                Salary50th = 45000,
                Salary75th = 60000,
                Source = "Glassdoor 2025",
                CreatedBy = "admin@singular.co.za",
                CreatedDate = DateTime.UtcNow
            };

            _mockRepository
                .Setup(r => r.CreateAsync(It.IsAny<SalaryBenchmark>()))
                .ReturnsAsync(fakeCreatedBenchmark);

            // ACT 
            var result = await _service.CreateAsync(request, "admin@singular.co.za");

            // ASSERT 
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Software Engineer", result.PositionTitle);
            Assert.Equal("Johannesburg", result.Location);
            Assert.Equal(45000, result.Salary50th);
        }

        [Fact]
        public async Task CreateAsync_CreatedByIsSetFromUsername_NotFromRequest()
        {
            // ARRANGE
            var request = new SalaryBenchmarkRequestDto
            {
                PositionId = 1,
                Location = "Cape Town",
                Salary25th = 20000,
                Salary50th = 30000,
                Salary75th = 40000,
                Source = "Payscale 2025"
            };
            SalaryBenchmark? capturedBenchmark = null;

            _mockRepository
                .Setup(r => r.CreateAsync(It.IsAny<SalaryBenchmark>()))
                .Callback<SalaryBenchmark>(b => capturedBenchmark = b) 
                .ReturnsAsync((SalaryBenchmark b) => b);

            // ACT
            await _service.CreateAsync(request, "bri@singular.co.za");

            // ASSERT 
            Assert.NotNull(capturedBenchmark);
            Assert.Equal("bri@singular.co.za", capturedBenchmark!.CreatedBy);
        }

        [Fact]
        public async Task CreateAsync_CreatedDateIsSetAutomatically()
        {
            // ARRANGE
            var request = new SalaryBenchmarkRequestDto
            {
                PositionId = 2,
                Location = "Johannesburg",
                Salary25th = 15000,
                Salary50th = 22000,
                Salary75th = 30000,
                Source = "Internal Survey"
            };

            SalaryBenchmark? capturedBenchmark = null;

            _mockRepository
                .Setup(r => r.CreateAsync(It.IsAny<SalaryBenchmark>()))
                .Callback<SalaryBenchmark>(b => capturedBenchmark = b)
                .ReturnsAsync((SalaryBenchmark b) => b);

            var beforeCall = DateTime.UtcNow;

            // ACT
            await _service.CreateAsync(request, "admin@singular.co.za");

            var afterCall = DateTime.UtcNow;

            // ASSERT 
            Assert.NotNull(capturedBenchmark);
            Assert.True(capturedBenchmark!.CreatedDate >= beforeCall);
            Assert.True(capturedBenchmark!.CreatedDate <= afterCall);
        }

        [Fact]
        public async Task GetAllEmployeesWithBenchmarks_WhenEmployeeHasBenchmark_ReturnsSalaryData()
        {
            // ARRANGE 
            var fakeEmployees = new List<EmployeeSalaryBenchmarkDto>
            {
                new EmployeeSalaryBenchmarkDto
                {
                    EmployeeId = "BSM001",
                    FullName = "Ben Smith",
                    PositionTitle = "Executive",
                    Location = "Johannesburg",
                    MonthlySalary = 50000,
                    Salary25th = 45000,
                    Salary50th = 60000,
                    Salary75th = 75000,
                    Source = "Glassdoor 2025"
                }
            };

            _mockRepository
                .Setup(r => r.GetEmployeeSalaryBenchmarksAsync())
                .ReturnsAsync(fakeEmployees);

            // ACT
            var result = (await _service.GetEmployeeSalaryBenchmarksAsync()).ToList();

            // ASSERT
            Assert.Single(result);                              // only one employee
            Assert.Equal("Ben Smith", result[0].FullName);
            Assert.Equal(45000, result[0].Salary25th);         // benchmark data is present
            Assert.Equal("Glassdoor 2025", result[0].Source);
        }

        [Fact]
        public async Task GetAllEmployeesWithBenchmarks_WhenEmployeeHasNoBenchmark_ReturnsNullSalaryFields()
        {
            // ARRANGE 
            var fakeEmployees = new List<EmployeeSalaryBenchmarkDto>
            {
                new EmployeeSalaryBenchmarkDto
                {
                    EmployeeId = "BSM002",
                    FullName = "Jane Doe",
                    PositionTitle = "Software Engineer",
                    Location = "Cape Town",
                    MonthlySalary = 38000,
                    Salary25th = null,      // no benchmark
                    Salary50th = null,
                    Salary75th = null,
                    Source = null
                }
            };

            _mockRepository
                .Setup(r => r.GetEmployeeSalaryBenchmarksAsync())
                .ReturnsAsync(fakeEmployees);

            // ACT
            var result = (await _service.GetEmployeeSalaryBenchmarksAsync()).ToList();

            // ASSERT 
            Assert.Single(result);
            Assert.Null(result[0].Salary25th);
            Assert.Null(result[0].Source);
        }

        [Fact]
        public async Task GetAllEmployeesWithBenchmarks_WhenNoEmployees_ReturnsEmptyList()
        {
            // ARRANGE
            _mockRepository
                .Setup(r => r.GetEmployeeSalaryBenchmarksAsync())
                .ReturnsAsync(new List<EmployeeSalaryBenchmarkDto>());

            // ACT
            var result = await _service.GetEmployeeSalaryBenchmarksAsync();

            // ASSERT 
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetSummaryAsync_ReturnsCorrectCounts()
        {
            // ARRANGE
            var fakeSummary = new BenchmarkSummaryDto
            {
                TotalBenchmarks = 12,
                TotalPositions = 6,
                Locations = 2
            };

            _mockRepository
                .Setup(r => r.GetSummaryAsync())
                .ReturnsAsync(fakeSummary);

            // ACT
            var result = await _service.GetSummaryAsync();

            // ASSERT
            Assert.Equal(12, result.TotalBenchmarks);
            Assert.Equal(6, result.TotalPositions);
            Assert.Equal(2, result.Locations);
        }

        [Fact]
        public async Task GetSummaryAsync_WhenNoBenchmarksExist_ReturnsZeroCounts()
        {
            // ARRANGE
            var fakeSummary = new BenchmarkSummaryDto
            {
                TotalBenchmarks = 0,
                TotalPositions = 0,
                Locations = 0
            };

            _mockRepository
                .Setup(r => r.GetSummaryAsync())
                .ReturnsAsync(fakeSummary);

            // ACT
            var result = await _service.GetSummaryAsync();

            // ASSERT 
            Assert.Equal(0, result.TotalBenchmarks);
            Assert.Equal(0, result.TotalPositions);
            Assert.Equal(0, result.Locations);
        }
    }
}