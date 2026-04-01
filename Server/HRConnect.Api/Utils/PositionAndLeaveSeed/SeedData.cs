namespace HRConnect.Api.Utils.PositionAndLeaveSeed
{
    using HRConnect.Api.Models;
    using System;
    using System.Security.Permissions;
    public static class SeedData
    {
        public static JobGrade[] GetJobGrades()
        {
            return new JobGrade[]
            {
                new JobGrade { JobGradeId = 1, Name = "Executive Director", IsActive = true, CreatedDate = new DateTime(2026, 2, 11, 14, 33, 32), UpdatedDate = new DateTime(2026, 2, 27, 10, 15, 8) },
                new JobGrade { JobGradeId = 2, Name = "Junior Management", IsActive = true, CreatedDate = new DateTime(2026, 2, 12, 7, 14, 45), UpdatedDate = new DateTime(2026, 2, 27, 6, 41, 39) },
                new JobGrade { JobGradeId = 3, Name = "Middle Management", IsActive = true, CreatedDate = new DateTime(2026, 2, 12, 7, 15, 20), UpdatedDate = new DateTime(2026, 2, 12, 7, 15, 20) },
                new JobGrade { JobGradeId = 4, Name = "Skilled/Semi Skilled", IsActive = true, CreatedDate = new DateTime(2026, 2, 12, 7, 15, 39), UpdatedDate = new DateTime(2026, 2, 12, 7, 15, 39) },
                new JobGrade { JobGradeId = 5, Name = "Top/Senior Management", IsActive = true, CreatedDate = new DateTime(2026, 2, 12, 7, 15, 54), UpdatedDate = new DateTime(2026, 2, 12, 7, 15, 54) },
                new JobGrade { JobGradeId = 6, Name = "Unskilled", IsActive = true, CreatedDate = new DateTime(2026, 2, 12, 7, 16, 8), UpdatedDate = new DateTime(2026, 2, 12, 7, 16, 8) },
            };
        }
        public static OccupationalLevel[] GetOccupationalLevels()
        {
            return new OccupationalLevel[]
            {
                new OccupationalLevel { OccupationalLevelId = 1, Description = "Top management", CreatedDate = new DateTime(2026,2,12,12,26,22), UpdatedDate = new DateTime(2026,2,23,13,45,35), IsActive = true },
                new OccupationalLevel { OccupationalLevelId = 2, Description = "Skilled technical and academically qualified workers, junior management, supervisors, foremen and superintendents", CreatedDate = new DateTime(2026,2,12,12,31,7), UpdatedDate = new DateTime(2026,2,23,13,51,57), IsActive = true },
                new OccupationalLevel { OccupationalLevelId = 3, Description = "Semi-skilled and discretionary decision making", CreatedDate = new DateTime(2026,2,12,12,26,59), UpdatedDate = new DateTime(2026,2,23,13,52,19), IsActive = true },
                new OccupationalLevel { OccupationalLevelId = 4, Description = "Professionally qualified, experienced specialists, and mid-management", CreatedDate = new DateTime(2026,2,13,12,54,57), UpdatedDate = new DateTime(2026,2,23,13,52,42), IsActive = true },
                new OccupationalLevel { OccupationalLevelId = 5, Description = "Senior management", CreatedDate = new DateTime(2026,2,12,12,29,24), UpdatedDate = new DateTime(2026,2,23,13,53,2), IsActive = true },
                new OccupationalLevel { OccupationalLevelId = 6, Description = "Unskilled and defined decision making", CreatedDate = new DateTime(2026,2,12,12,30,2), UpdatedDate = new DateTime(2026,2,23,13,53,25), IsActive = true },
            };
        }
        public static Position[] GetPositions()
        {
            return new Position[]
            {
            new Position { PositionId = 1, PositionTitle = "Chief Financial Officer", JobGradeId = 1, OccupationalLevelId = 1, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 2, PositionTitle = "Chief Operating Officer", JobGradeId = 1, OccupationalLevelId = 1, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 3, PositionTitle = "Officer", JobGradeId = 1, OccupationalLevelId = 1, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 4, PositionTitle = "Executive", JobGradeId = 1, OccupationalLevelId = 1, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 5, PositionTitle = "Founder", JobGradeId = 1, OccupationalLevelId = 1, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 6, PositionTitle = "Head: Financial Services", JobGradeId = 1, OccupationalLevelId = 1, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 7, PositionTitle = "Accountant", JobGradeId = 2, OccupationalLevelId = 2, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 8, PositionTitle = "Admin Manager", JobGradeId = 2, OccupationalLevelId = 2, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 9, PositionTitle = "Analyst", JobGradeId = 2, OccupationalLevelId = 2, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 10, PositionTitle = "Associate Data Analyst", JobGradeId = 2, OccupationalLevelId = 2, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 11, PositionTitle = "Associate Project Analyst", JobGradeId = 2, OccupationalLevelId = 2, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 12, PositionTitle = "Associate Software Engineer", JobGradeId = 2, OccupationalLevelId = 2, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 13, PositionTitle = "Bookkeeper", JobGradeId = 2, OccupationalLevelId = 2, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 14, PositionTitle = "Client Liason Manager", JobGradeId = 2, OccupationalLevelId =3, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 17, PositionTitle = "Data Analyst", JobGradeId = 2, OccupationalLevelId = 2, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1)},
            new Position { PositionId = 18, PositionTitle = "Finance Administrator", JobGradeId = 2, OccupationalLevelId = 4, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 19, PositionTitle = "Finance Agent 1", JobGradeId = 2, OccupationalLevelId = 2, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 20, PositionTitle = "Finance Agent 2", JobGradeId = 2, OccupationalLevelId = 2, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 21, PositionTitle = "Finance Agent 3", JobGradeId = 2, OccupationalLevelId = 2, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 22, PositionTitle = "Finance Supervisor", JobGradeId = 2, OccupationalLevelId = 2, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) },
            new Position { PositionId = 23, PositionTitle = "INACTIVE Software Developer", JobGradeId = 2, OccupationalLevelId = 2, IsActive = true, CreatedDate = new DateTime(2026, 1, 1), UpdatedDate = new DateTime(2026, 1, 1) }
            };
        }

        public static LeaveType[] GetLeaveTypes()
        {
            return new LeaveType[]
            {
                 new LeaveType
          {
            Id = 1,
            Name = "Annual Leave",
            Code = "AL",
            Description = "Annual Leave Policy",
            ResetMonth = 1,
            ResetDay = 1,
            MaxCarryoverDays = 5,
            CarryoverExpiryMonth = 1,
            CarryoverExpiryDay = 1,
            CarryoverNotificationMonth = 12,
            CarryoverNotificationDay = 1,
            IsRollingWindow = false,
            RollingMonths = null,
            FemaleOnly = false,
            IsActive = true
          },
          new LeaveType
          {
            Id = 2,
            Name = "Sick Leave",
            Code = "SL",
            Description = "Sick Leave Policy",
            ResetMonth = null,
            ResetDay = null,
            IsRollingWindow = true,
            RollingMonths = 36,
            FemaleOnly = false,
            IsActive = true
          },
          new LeaveType
          {
            Id = 3,
            Name = "Maternity Leave",
            Code = "ML",
            Description = "Maternity Leave Policy",
            FemaleOnly = true,
            IsRollingWindow = false,
            IsActive = true
          },
          new LeaveType
          {
            Id = 4,
            Name = "Family Responsibility Leave",
            Code = "FRL",
            Description = "Family Responsibility Policy",
            IsRollingWindow = true,
            RollingMonths = 12,
            FemaleOnly = false,
            IsActive = true
          }
            };
        }

        public static LeaveEntitlementRule[] GetLeaveEntitlementRules()
        {
            return new LeaveEntitlementRule[]
            {
               // ===== GROUP A (2,3,4,6 SAME) =====
                // <3 years
                new LeaveEntitlementRule { Id = 1, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 15, IsActive = true },
                new LeaveEntitlementRule { Id = 2, LeaveTypeId = 1, JobGradeId = 3, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 15, IsActive = true },
                new LeaveEntitlementRule { Id = 3, LeaveTypeId = 1, JobGradeId = 4, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 15, IsActive = true },
                new LeaveEntitlementRule { Id = 4, LeaveTypeId = 1, JobGradeId = 6, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 15, IsActive = true },
                // 3–5 years
                new LeaveEntitlementRule { Id = 5, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 18, IsActive = true },
                new LeaveEntitlementRule { Id = 6, LeaveTypeId = 1, JobGradeId = 3, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 18, IsActive = true },
                new LeaveEntitlementRule { Id = 7, LeaveTypeId = 1, JobGradeId = 4, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 18, IsActive = true },
                new LeaveEntitlementRule { Id = 8, LeaveTypeId = 1, JobGradeId = 6, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 18, IsActive = true },

                // >5 years
                new LeaveEntitlementRule { Id = 9, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 20, IsActive = true },
                new LeaveEntitlementRule { Id = 10, LeaveTypeId = 1, JobGradeId = 3, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 20, IsActive = true },
                new LeaveEntitlementRule { Id = 11, LeaveTypeId = 1, JobGradeId = 4, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 20, IsActive = true },
                new LeaveEntitlementRule { Id = 12, LeaveTypeId = 1, JobGradeId = 6, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 20, IsActive = true },

                // ===== GROUP B (5) =====
                new LeaveEntitlementRule { Id = 13, LeaveTypeId = 1, JobGradeId = 5, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 18, IsActive = true },
                new LeaveEntitlementRule { Id = 14, LeaveTypeId = 1, JobGradeId = 5, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 21, IsActive = true },
                new LeaveEntitlementRule { Id = 15, LeaveTypeId = 1, JobGradeId = 5, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 23, IsActive = true },

                // ===== GROUP C (1) =====
                new LeaveEntitlementRule { Id = 16, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 22, IsActive = true },
                new LeaveEntitlementRule { Id = 17, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 25, IsActive = true },
                new LeaveEntitlementRule { Id = 18, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 27, IsActive = true }
            };

        }

    }

}
