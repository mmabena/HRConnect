namespace HRConnect.Api.Utils.Seed
{
  using HRConnect.Api.Data;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;

  public class PositionAndLeaveSeed
  {
    private readonly ApplicationDBContext _context;

    //Seed Data we need
    private readonly List<JobGrade> _seedJobGrade = new()
    {
       //DON'T MANUALLY SET IDs
        new JobGrade
        {
          // JobGradeId=1,
          Name="Executive Director",
          IsActive=true,
        },
        new JobGrade
        {
          // JobGradeId=2,
          Name="Junior Management",
          IsActive=true
        },
        new JobGrade
        {
          // JobGradeId=3,
          Name="Middle Management",
          IsActive=true
        },
        new JobGrade
        {
          // JobGradeId=4,
          Name="Skiiled/Semi Skilled",
          IsActive=true
        },
        new JobGrade
        {
          // JobGradeId=5,
          Name="Top/Senior Management",
          IsActive=true
        },
        new JobGrade
        {
          // JobGradeId=6,
          Name="Unskilled",
          IsActive=true
        }
    };

    private readonly List<OccupationalLevel> _seedOccuptationLevel = new()
    {
      new OccupationalLevel
      {
        // OccupationalLevelId=1,
        Description="Top Management",
        IsActive=true
      },
      new OccupationalLevel
      {
        // OccupationalLevelId=2,
        Description="Senior Management",
        IsActive=true
      }
    };

    private readonly List<LeaveEntitlementRule> _seedLeaveEntitlementRules = new()
{
    // ================= ANNUAL LEAVE =================

    // <3 years (Unskilled–Middle: 15)
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 15, IsActive = true },
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 3, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 15, IsActive = true },
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 4, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 15, IsActive = true },
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 6, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 15, IsActive = true },

    // <3 years (Senior Management)
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 5, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 18, IsActive = true },

    // <3 years (Executive Director)
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 22, IsActive = true },

    // 3–5 years
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 18, IsActive = true },
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 3, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 18, IsActive = true },
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 4, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 18, IsActive = true },
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 6, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 18, IsActive = true },

    // Senior Management
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 5, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 21, IsActive = true },

    // Executive Director
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 25, IsActive = true },

    // >5 years
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 20, IsActive = true },
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 3, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 20, IsActive = true },
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 4, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 20, IsActive = true },
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 6, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 20, IsActive = true },

    // Senior Management
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 5, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 23, IsActive = true },

    // Executive Director
    new LeaveEntitlementRule { LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 27, IsActive = true },

   //SICK LEAVE
    new LeaveEntitlementRule
    {
        LeaveTypeId = 2,
        JobGradeId = null,
        MinYearsService = 0,
        MaxYearsService = null,
        DaysAllocated = 30,
        IsActive = true
    },
    
    //MATERNITY LEAVE
    new LeaveEntitlementRule
    {
        LeaveTypeId = 3,
        JobGradeId = null,
        MinYearsService = 0,
        MaxYearsService = null,
        DaysAllocated = 120,
        IsActive = true
    },
    
    //FAMILY RESPONSIBILITY
    new LeaveEntitlementRule
    {
        LeaveTypeId = 4,
        JobGradeId = null,
        MinYearsService = 0,
        MaxYearsService = null,
        DaysAllocated = 3,
        IsActive = true
    }
    };
    private readonly List<LeaveType> _seedLeaveTypes = new()
    {
      new LeaveType
      {
        // Id=1,
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
        FemaleOnly = false,
        IsActive = true
      }, new LeaveType
      {
        // Id=2,
        Name = "Sick Leave",
        Code = "SL",
        Description = "Sick Leave Policy",
        IsRollingWindow = true,
        RollingMonths = 36,
        FemaleOnly = false,
        IsActive = true
      },
       new LeaveType
      {
        // Id=3,
        Name = "Maternity Leave",
        Code = "ML",
        Description = "Maternity Leave Policy",
        FemaleOnly = true,
        IsRollingWindow = false,
        IsActive = true
      },
      new LeaveType
       {
        // Id=4,
        Name = "Family Responsibility Leave",
        Code = "FRL",
        Description = "Family Responsibility Policy",
        IsRollingWindow = true,
        RollingMonths = 12,
        FemaleOnly = false,
        IsActive = true
       }
    };

    private readonly List<PensionOption> _seedPensionOptions = new()
    {
      new PensionOption
      {
        // PensionOptionId=1,
        ContributionPercentage = 5.00m
      },
      new PensionOption
      {
        // PensionOptionId=2,
        ContributionPercentage = 3.00m
      },
    };

    public PositionAndLeaveSeed(ApplicationDBContext context)
    {
      _context = context;
    }

    public async Task SeedAsync()
    {
      await SeedJobGrade();
      await SeedOccuptationLevel();
      await SeedLeaveTypes();
      await SeedLeaveEntitlementRules();

      //temporarily seed pension options 
      await SeedPensionOptions();
    }

    //Seed the Job Grade first using transaction
    public async Task SeedJobGrade()
    {
      if (!await _context.JobGrades.AnyAsync())
      {
        //Allow explicit insertions with IDs

        //reset ID for seeding
        _ = await _context.Database.ExecuteSqlRawAsync(
         "DBCC CHECKIDENT ('JobGrades', RESEED, 0)");

        await _context.JobGrades.AddRangeAsync(_seedJobGrade);
        _ = await _context.SaveChangesAsync();

      }
    }

    //Seed OccupationalLevel 
    public async Task SeedOccuptationLevel()
    {
      if (!await _context.OccupationalLevels.AnyAsync())
      {
        //reset ID for seeding
        _ = await _context.Database.ExecuteSqlRawAsync(
         "DBCC CHECKIDENT ('OccupationalLevels', RESEED, 0)");

        _context.OccupationalLevels.AddRange(_seedOccuptationLevel);

        _ = await _context.SaveChangesAsync();
      }
    }
    //Seed LeaveEntitlementRules
    public async Task SeedLeaveEntitlementRules()
    {
      if (!await _context.LeaveEntitlementRules.AnyAsync())
      {
        //reset ID for seeding
        _ = await _context.Database.ExecuteSqlRawAsync(
         "DBCC CHECKIDENT ('LeaveEntitlementRules', RESEED, 0)");

        await _context.LeaveEntitlementRules.AddRangeAsync(_seedLeaveEntitlementRules);

        _ = await _context.SaveChangesAsync();
      }
    }
    //Seed LeaveTypes
    public async Task SeedLeaveTypes()
    {
      if (!await _context.LeaveTypes.AnyAsync())
      {
        //reset ID for seeding
        _ = await _context.Database.ExecuteSqlRawAsync(
         "DBCC CHECKIDENT ('LeaveTypes', RESEED, 0)");

        await _context.LeaveTypes.AddRangeAsync(_seedLeaveTypes);

        _ = await _context.SaveChangesAsync();
      }
    }
    public async Task SeedPensionOptions()
    {
      if (!await _context.PensionOptions.AnyAsync())
      {
        //reset ID for seeding
        _ = await _context.Database.ExecuteSqlRawAsync(
         "DBCC CHECKIDENT ('PensionOptions', RESEED, 0)");

        await _context.PensionOptions.AddRangeAsync(_seedPensionOptions);

        _ = await _context.SaveChangesAsync();
      }
    }
  }
}