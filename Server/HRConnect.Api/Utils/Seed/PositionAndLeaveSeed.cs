namespace HRConnect.Api.Utils.Seed
{

  using HRConnect.Api.Data;
  using HRConnect.Api.Repository;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;

  public class PositionAndLeaveSeed
  {
    //Repos to add seed value
    private readonly ApplicationDBContext _context;
    private readonly JobGradeRepository _jobGradeRepo;
    private readonly IPositionRepository _positionRepo;
    private readonly IOccupationalLevelRepository _occupationalLevelRepo;
    private readonly ILeaveTypeManagementRepository _leaveTypeManagementRepo;
    //services to follow correct code path
    private readonly ILeaveTypeManagementService _leaveTypeManagementService;

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
        Name = "Family Responsibility Leave",
        Code = "FRL",
        Description = "Family Responsibility Policy",
        IsRollingWindow = true,
        RollingMonths = 12,
        FemaleOnly = false,
        IsActive = true
       }
    };
    public PositionAndLeaveSeed(ApplicationDBContext context, JobGradeRepository jobGradeRepo,
        IPositionRepository positionRepo, ILeaveTypeManagementService leaveTypeManagementService,
        IOccupationalLevelRepository occupationalLevelRepo, ILeaveTypeManagementRepository leaveTypeManagementRepo)
    {
      _context = context;
      _jobGradeRepo = jobGradeRepo;
      _positionRepo = positionRepo;
      _leaveTypeManagementService = leaveTypeManagementService;
      _occupationalLevelRepo = occupationalLevelRepo;
      _leaveTypeManagementRepo = leaveTypeManagementRepo;
    }

    //Seed the Job Grade first
    public async Task SeedJobGrade()
    {
      foreach (var j in _seedJobGrade)
      {
        await _jobGradeRepo.AddJobGradeAsync(j);
      }
    }

    //Seed OccupationalLevel 
    public async Task SeedOccuptationLevel()
    {
      foreach (var o in _seedOccuptationLevel)
      {
        await _occupationalLevelRepo.AddOccupationalLevelAsync(o);
      }
    }

    //Seed Positions
    public async Task SeedPositions()
    {
      foreach (var p in _seedPositions)
      {
        await _positionRepo.AddPositionAsync(p);
      }
    }

    //Seed LeaveTypes
    public async Task SeedLeaveTypes()
    {
      foreach (var s in _seedLeaveTypes)
      {
        await _leaveTypeManagementRepo.CreateLeaveTypeAsync(s);
      }
    }
  }

}