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
        [Fact]
        public async Task CreateGroupAsync_ShouldCreateGroupSuccessfully()
        {
            var db = GetDb();

            db.JobGrades.AddRange(
                new JobGrade { JobGradeId = 1, Name = "Grade A" },
                new JobGrade { JobGradeId = 2, Name = "Grade B" }
            );
            await db.SaveChangesAsync();

            var service = GetService(db);

            await service.CreateGroupAsync(
                new CreateGroupRequest
                {
                    GroupKey = "GROUP_A",
                    JobGradeIds = new List<int> { 1, 2 }
                }
            );
            Assert.Equal(2, db.JobGradeGroupMaps.Count());
            Assert.All(db.JobGradeGroupMaps,
            x => Assert.Equal("GROUP_A", x.GroupKey));
        }
        [Fact]
        public async Task CreateGroupAsync_ShouldThrow_WhenGroupKeyMissing()
        {
            var db = GetDb();
            var service = GetService(db);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateGroupAsync(
                new CreateGroupRequest
                {
                    GroupKey = "",
                    JobGradeIds = new List<int>()
                }
            ));
        }
        [Fact]
        public async Task CreateGroupAsync_ShouldThrow_WhenGroupAlreadyExists()
        {
            var db = GetDb();

            db.JobGrades.Add(
                new JobGrade { JobGradeId = 1, Name = "Grade A" }
            );
            db.JobGradeGroupMaps.Add(
                new JobGradeGroupMap { GroupKey = "GROUP_A", JobGradeId = 1 }
            );
            await db.SaveChangesAsync();
            var service = GetService(db);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateGroupAsync(
                    new CreateGroupRequest
                    {
                        GroupKey = "GROUP_A",
                        JobGradeIds = new List<int> { 1 }
                    }
                ));
        }
        [Fact]
        public async Task CreateGroupAsync_ShouldThrow_WhenJobGradeDoesNotExist()
        {
            var db = GetDb();

            var service = GetService(db);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateGroupAsync(
                new CreateGroupRequest
                {
                    GroupKey = "GROUP_A",
                    JobGradeIds = new List<int> { 876 }
                }
            ));
        }
        [Fact]
        public async Task CreateGroupAsync_ShouldThrow_WhenJobGradeAlreadyAssigned()
        {
            var db = GetDb();

            db.JobGrades.AddRange(
                new JobGrade { JobGradeId = 1, Name = "Grade A" },
                new JobGrade { JobGradeId = 2, Name = "Grade B" }
            );
            db.JobGradeGroupMaps.Add(
                new JobGradeGroupMap
                {
                    JobGradeId = 1,
                    GroupKey = "GROUP_A"
                }
            );
            await db.SaveChangesAsync();

            var service = GetService(db);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateGroupAsync(
                new CreateGroupRequest
                {
                    GroupKey = "GROUP_B",
                    JobGradeIds = new List<int> { 1, 2 }
                }
            ));
        }
        [Fact]
        public async Task MoveJobGrade_ShouldMoveJobGradeSuccessFully()
        {
            var db = GetDb();

            db.JobGradeGroupMaps.AddRange(
               new JobGradeGroupMap
               {
                   JobGradeId = 1,
                   GroupKey = "GROUP_A"
               },
               new JobGradeGroupMap
               {
                   JobGradeId = 2,
                   GroupKey = "GROUP_A"
               },
               new JobGradeGroupMap
               {
                   JobGradeId = 3,
                   GroupKey = "GROUP_B"
               });
            await db.SaveChangesAsync();
            var service = GetService(db);
            await service.MoveJobGradeAsync(
             new MoveJobGradeRequest
             {
                 JobGradeId = 1,
                 NewGroupKey = "GROUP_B"
             }
            );
            var updated = await db.JobGradeGroupMaps
                .FirstAsync(x => x.JobGradeId == 1);
            Assert.Equal("GROUP_B", updated.GroupKey);
        }
        [Fact]
        public async Task MoveJobGradeAsync_ShouldThrow_WhenMappingIsNotFound()
        {
            var db = GetDb();

            var service = GetService(db);
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.MoveJobGradeAsync(
                new MoveJobGradeRequest
                {
                    JobGradeId = 876,
                    NewGroupKey = "GROUP_A"
                }
            ));
        }
        [Fact]
        public static async Task MoveJobGradeAsync_ShouldThrow_WhenTargetGroupMissing()
        {
            var db = GetDb();
            db.JobGradeGroupMaps.AddRange(
                new JobGradeGroupMap
                {
                    JobGradeId = 1,
                    GroupKey = "GROUP_A"
                },
                new JobGradeGroupMap
                {
                    JobGradeId = 2,
                    GroupKey = "GROUP_B"
                }
            );
            await db.SaveChangesAsync();
            var service = GetService(db);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.MoveJobGradeAsync(
                    new MoveJobGradeRequest
                    {
                        JobGradeId = 1,
                        NewGroupKey = "GROUP_C"
                    }
                ));
        }
        [Fact]
        public async Task MoveJobGradeAsync_ShouldThrow_WhenMovingLastItemInGroup()
        {
            var db = GetDb();
            db.JobGradeGroupMaps.AddRange(
                new JobGradeGroupMap
                {
                    JobGradeId = 1,
                    GroupKey = "GROUP_A"
                },
                new JobGradeGroupMap
                {
                    JobGradeId = 2,
                    GroupKey = "GROUP_B"
                }
            );
            await db.SaveChangesAsync();
            var service = GetService(db);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.MoveJobGradeAsync(
                    new MoveJobGradeRequest
                    {
                        JobGradeId = 1,
                        NewGroupKey = "GROUP_B"
                    }
                ));
        }
        [Fact]
        public async Task RemoveJobGrade_ShouldRemoveSuccessfully()
        {
            var db = GetDb();
            db.JobGradeGroupMaps.AddRange(
                new JobGradeGroupMap
                {
                    JobGradeId = 1,
                    GroupKey = "GROUP_A"
                },
                new JobGradeGroupMap
                {
                    JobGradeId = 2,
                    GroupKey = "GROUP_A"
                }
            );
            await db.SaveChangesAsync();
            var service = GetService(db);
            await service.RemoveJobGradeAsync(
                new RemoveJobGradeRequest
                {
                    JobGradeId = 1
                }
            );
            Assert.Single(db.JobGradeGroupMaps);
        }
        [Fact]
        public async Task RemoveJobGradeAsync_ShouldThrow_WhenMappingIsNotFound()
        {
            var db = GetDb();
            var service = GetService(db);
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.RemoveJobGradeAsync(
                    new RemoveJobGradeRequest
                    {
                        JobGradeId = 56
                    }
                ));
        }
        [Fact]
        public async Task RemoveJobGradeAsync_ShouldThrow_WhenRemoveLastJobGradeFromGroup()
        {
            var db = GetDb();
            db.JobGradeGroupMaps.Add(
                new JobGradeGroupMap
                {
                    JobGradeId = 1,
                    GroupKey = "GROUP_A"
                }
            );
            await db.SaveChangesAsync();
            var service = GetService(db);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.RemoveJobGradeAsync(
                    new RemoveJobGradeRequest
                    {
                        JobGradeId = 1
                    }
                ));
        }
    }
}