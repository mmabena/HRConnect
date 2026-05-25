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
    public class JobGradeGroupServiceTests
    {
        public static ApplicationDBContext GetDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDBContext(options);
        }
        private static JobGradeGroupService GetService(ApplicationDBContext db)
        {
            return new JobGradeGroupService(db);
        }
        [Fact]
        public async Task GetGroupsAsync_ShouldReturnGroupedResults()
        {
            var db = GetDb();

            db.JobGrades.AddRange(
                new JobGrade { JobGradeId = 1, Name = "Grade A" },
                new JobGrade { JobGradeId = 2, Name = "Grade B" }
            );
            db.JobGradeGroupMaps.AddRange(
                new JobGradeGroupMap { JobGradeId = 1, GroupKey = "GROUP_A" },
                new JobGradeGroupMap { JobGradeId = 2, GroupKey = "GROUP_A" }
            );
            await db.SaveChangesAsync();
            var service = GetService(db);
            var result = await service.GetGroupsAsync();
            Assert.Single(result);
            Assert.Equal("GROUP_A", result[0].GroupKey);
            Assert.Equal(2, result[0].JobGrades.Count);
        }
    }
}