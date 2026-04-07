namespace HRConnect.Api.Utils.Seed
{

  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using Microsoft.EntityFrameworkCore;

  public class PositionAndLeaveSeed
  {
    private readonly ApplicationDBContext _context;
    //Repos to add seed value
    private readonly IJobGradeRepository _jobGradeRepo;
    private readonly IPositionRepository _positionRepo;
    private readonly IOccupationalLevelRepository _occupationalLevelRepo;
    private readonly ILeaveTypeManagementRepository _leaveTypeManagementRepo;
    //services to follow correct code path

    //Seed Data we need
    private readonly List<JobGrade> _seedJobGrade = new()
    {
       //DON'T MANUALLY SET IDs
        new JobGrade
        {
          Name="Executive Director",
          IsActive=true,
        },
        new JobGrade
        {
          Name="Junior Management",
          IsActive=true
        },
        new JobGrade
        {
          Name="Middle Management",
          IsActive=true
        },
        new JobGrade
        {
          Name="Skiiled/Semi Skilled",
          IsActive=true
        },
        new JobGrade
        {
          Name="Top/Senior Management",
          IsActive=true
        },
        new JobGrade
        {
          Name="Unskilled",
          IsActive=true
        }
    };

    private readonly List<Position> _seedPositions = new()
    {
      new Position
      {
        PositionTitle="Executive",
        JobGradeId=1,
        IsActive=true,
        OccupationalLevelId=1,
        CreatedDate=new DateTime(2026,1,1)
      },
      new Position
      {
        PositionTitle="Analyst",
        JobGradeId=2,
        IsActive=true,
        OccupationalLevelId=2,
        CreatedDate=new DateTime(2026,1,1)
      }
    };

    private readonly List<OccupationalLevel> _seedOccuptationLevel = new()
    {
      new OccupationalLevel
      {
        Description="Top Management",
        IsActive=true
      },
      new OccupationalLevel
      {
        Description="Senior Management",
        IsActive=true
      }
    };

    private readonly List<LeaveEntitlementRule> _seedLeaveEntitlementRules = new()
    {
    // <3 years
    new LeaveEntitlementRule {  LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 15, IsActive = true },
    new LeaveEntitlementRule {  LeaveTypeId = 1, JobGradeId = 3, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 15, IsActive = true },
    new LeaveEntitlementRule {  LeaveTypeId = 1, JobGradeId = 4, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 15, IsActive = true },
    new LeaveEntitlementRule {  LeaveTypeId = 1, JobGradeId = 6, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 15, IsActive = true },

    // 3-5 years
    new LeaveEntitlementRule { Id = 5, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 18, IsActive = true },
    new LeaveEntitlementRule { Id = 6, LeaveTypeId = 1, JobGradeId = 3, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 18, IsActive = true },
    new LeaveEntitlementRule { Id = 7, LeaveTypeId = 1, JobGradeId = 4, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 18, IsActive = true },
    new LeaveEntitlementRule { Id = 8, LeaveTypeId = 1, JobGradeId = 6, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 18, IsActive = true },

    // >5 years
    new LeaveEntitlementRule { Id = 9, LeaveTypeId = 1, JobGradeId = 2, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 20, IsActive = true },
    new LeaveEntitlementRule { Id = 10, LeaveTypeId = 1, JobGradeId = 3, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 20, IsActive = true },
    new LeaveEntitlementRule { Id = 11, LeaveTypeId = 1, JobGradeId = 4, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 20, IsActive = true },
    new LeaveEntitlementRule { Id = 12, LeaveTypeId = 1, JobGradeId = 6, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 20, IsActive = true },

    // GROUP B (5)
    new LeaveEntitlementRule { Id = 13, LeaveTypeId = 1, JobGradeId = 5, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 18, IsActive = true },
    new LeaveEntitlementRule { Id = 14, LeaveTypeId = 1, JobGradeId = 5, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 21, IsActive = true },
    new LeaveEntitlementRule { Id = 15, LeaveTypeId = 1, JobGradeId = 5, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 23, IsActive = true },

    // GROUP C (1)
    new LeaveEntitlementRule { Id = 16, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 0, MaxYearsService = 2.99m, DaysAllocated = 22, IsActive = true },
    new LeaveEntitlementRule { Id = 17, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 3, MaxYearsService = 5, DaysAllocated = 25, IsActive = true },
    new LeaveEntitlementRule { Id = 18, LeaveTypeId = 1, JobGradeId = 1, MinYearsService = 5.01m, MaxYearsService = null, DaysAllocated = 27, IsActive = true }

    };



    private readonly List<LeaveType> _seedLeaveTypes = new()
    {
      new LeaveType
      {
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
        Name = "Maternity Leave",
        Code = "ML",
        Description = "Maternity Leave Policy",
        FemaleOnly = true,
        IsRollingWindow = false,
        IsActive = true
      },
      new LeaveType
       {
        Name = "Family Responsibility Leave",
        Code = "FRL",
        Description = "Family Responsibility Policy",
        IsRollingWindow = true,
        RollingMonths = 12,
        FemaleOnly = false,
        IsActive = true
       }
    };
    public PositionAndLeaveSeed(ApplicationDBContext context, IJobGradeRepository jobGradeRepo, IPositionRepository positionRepo,
    IOccupationalLevelRepository occupationalLevelRepo, ILeaveTypeManagementRepository leaveTypeManagementRepo)
    {
      _context = context;
      _jobGradeRepo = jobGradeRepo;
      _positionRepo = positionRepo;
      _occupationalLevelRepo = occupationalLevelRepo;
      _leaveTypeManagementRepo = leaveTypeManagementRepo;
    }

    public async Task SeedAsync()
    {
      await SeedJobGrade();
      await SeedOccuptationLevel();
      await SeedPositions();
      await SeedLeaveTypes();
    }

    //Seed the Job Grade first
    public async Task SeedJobGrade()
    {
      JobGrade? job;
      foreach (var e in _seedJobGrade)
      {
        if (!await _context.JobGrades.AnyAsync(j => j.Name == e.Name))
        {
          job = e;
          _ = await _jobGradeRepo.AddJobGradeAsync(job);
        }
      }
    }

    //Seed OccupationalLevel 
    public async Task SeedOccuptationLevel()
    {
      OccupationalLevel? occupation;
      foreach (var e in _seedOccuptationLevel)
      {
        if (!await _context.OccupationalLevels.AnyAsync(o => o.Description == e.Description))
        {
          occupation = e;
          await _occupationalLevelRepo.AddOccupationalLevelAsync(occupation);
        }
      }
    }

    //Seed Positions
    public async Task SeedPositions()
    {
      Position? position;
      foreach (var e in _seedPositions)
      {
        if (!await _context.Positions.AnyAsync(p => p.PositionTitle == e.PositionTitle))
        {
          {
            position = e;
            _ = await _positionRepo.AddPositionAsync(position);
          }
        }
      }
    }

    //Seed LeaveTypes
    public async Task SeedLeaveTypes()
    {
      LeaveType? type;
      foreach (var e in _seedLeaveTypes)
      {
        if (!await _context.LeaveTypes.AnyAsync(l => l.Description == e.Description))
        {
          type = e;
          _ = await _leaveTypeManagementRepo.CreateLeaveTypeAsync(type);
        }
      }
    }
  }

}