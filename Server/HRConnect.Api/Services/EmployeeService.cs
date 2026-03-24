namespace HRConnect.Api.Services
{
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;
    using HRConnect.Api.Utils;

    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDBContext _context;
        private readonly IEmailService _emailService;
        private readonly ILeaveBalanceService _leaveBalanceService;
        private readonly ILeaveProcessingService _leaveProcessingService;

        public EmployeeService(
            ApplicationDBContext context,
            IEmailService emailService,
            ILeaveBalanceService leaveBalanceService,
            ILeaveProcessingService leaveProcessingService)
        {
            _context = context;
            _emailService = emailService;
            _leaveBalanceService = leaveBalanceService;
            _leaveProcessingService = leaveProcessingService;
        }

        public async Task<EmployeeResponse> CreateEmployeeAsync(CreateEmployeeRequest request)
        {
            var employee = new Employee
            {
                EmployeeId = Guid.NewGuid().ToString(),
                PositionId = request.PositionId,
                CareerManagerID = request.CareerManagerID,
                Name = request.Name,
                Surname = request.Surname,
                Email = request.Email,
                Gender = request.Gender,
                StartDate = request.StartDate,
                CreatedDate = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            await _leaveBalanceService.InitializeEmployeeLeaveBalancesAsync(employee.EmployeeId);

            return await GetEmployeeByIdAsync(employee.EmployeeId)
                   ?? throw new InvalidOperationException("Failed to load created employee.");
        }

        public async Task<List<EmployeeResponse>> GetAllEmployeesAsync()
        {
            var employeeIds = await _context.Employees
                .Select(e => e.EmployeeId)
                .ToListAsync();

            foreach (var id in employeeIds)
            {
                await _leaveBalanceService.RecalculateAnnualLeaveAsync(id);
            }

            await _leaveProcessingService.RecalculateAllSickLeaveAsync();
            await _leaveProcessingService.RecalculateAllFamilyResponsibilityLeaveAsync();

            var employees = await _context.Employees
                .Include(e => e.Position)
                    .ThenInclude(p => p.JobGrade)
                .Include(e => e.LeaveBalances)
                    .ThenInclude(lb => lb.LeaveType)
                .ToListAsync();

            return employees.Select(MapToResponse).ToList();
        }

        public async Task<EmployeeResponse?> GetEmployeeByIdAsync(string id)
        {
            await _leaveBalanceService.RecalculateAnnualLeaveAsync(id);
            await _leaveProcessingService.RecalculateAllSickLeaveAsync();
            await _leaveProcessingService.RecalculateAllFamilyResponsibilityLeaveAsync();

            var employee = await _context.Employees
                .Include(e => e.Position)
                    .ThenInclude(p => p.JobGrade)
                .Include(e => e.LeaveBalances)
                    .ThenInclude(lb => lb.LeaveType)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
                return null;

            return MapToResponse(employee);
        }

        public async Task<EmployeeResponse> UpdateEmployeePositionAsync(string employeeId, int newPositionId)
        {
            var employee = await _context.Employees
                .Include(e => e.Position)
                    .ThenInclude(p => p.JobGrade)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            if (employee.PositionId == newPositionId)
                return await GetEmployeeByIdAsync(employeeId)
                       ?? throw new InvalidOperationException("Failed to load employee.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var currentSegment = await _context.EmployeeAccrualRateHistories
                .Where(x => x.EmployeeId == employeeId && x.EffectiveTo == null)
                .FirstOrDefaultAsync();

            if (currentSegment != null)
            {
                if (currentSegment.EffectiveFrom == today)
                {
                    _context.EmployeeAccrualRateHistories.Remove(currentSegment);
                }
                else
                {
                    currentSegment.EffectiveTo = today.AddDays(-1);
                }
            }

            employee.PositionId = newPositionId;
            employee.UpdatedDate = DateTime.UtcNow;

            await _context.Entry(employee)
                .Reference(e => e.Position)
                .LoadAsync();

            await _context.Entry(employee.Position)
                .Reference(p => p.JobGrade)
                .LoadAsync();

            var annualLeave = await _context.LeaveTypes
                .FirstAsync(l => l.Code == "AL" && l.IsActive);

            var yearsOfService = CalculateYearsOfService(employee.StartDate);

            var newRule = await _context.LeaveEntitlementRules
                .FirstAsync(r =>
                    r.LeaveTypeId == annualLeave.Id &&
                    r.JobGradeId == employee.Position.JobGradeId &&
                    r.MinYearsService <= yearsOfService &&
                    (r.MaxYearsService == null || r.MaxYearsService >= yearsOfService) &&
                    r.IsActive);

            await _context.EmployeeAccrualRateHistories.AddAsync(
                new EmployeeAccrualRateHistory
                {
                    EmployeeId = employeeId,
                    PositionId = employee.PositionId,
                    AnnualEntitlement = newRule.DaysAllocated,
                    DailyRate = (newRule.DaysAllocated / 12m) / 21.67m,
                    EffectiveFrom = today,
                    EffectiveTo = null,
                    CreatedDate = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();

            await _leaveBalanceService.RecalculateAnnualLeaveAsync(employeeId);

            var balance = await _context.EmployeeLeaveBalances
                .Include(b => b.LeaveType)
                .FirstAsync(b =>
                    b.EmployeeId == employeeId &&
                    b.LeaveType.Code == "AL");

            try
            {
                var emailBody = EmailTemplates.GeneratePositionUpdateEmail(
                      employee,
                      balance.AccruedDays,
                      balance.TakenDays,
                      balance.AvailableDays
                  );

                await _emailService.SendEmailAsync(
                    employee.Email,
                    "Annual Leave Recalculated Due to Position Change",
                    emailBody
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }

            return await GetEmployeeByIdAsync(employeeId)
                   ?? throw new InvalidOperationException("Failed to load updated employee.");
        }

        private EmployeeResponse MapToResponse(Employee e)
        {
            var annual = e.LeaveBalances
                .FirstOrDefault(lb => lb.LeaveType.Code == "AL");

            return new EmployeeResponse
            {
                Id = e.EmployeeId,
                FullName = $"{e.Name} {e.Surname}",
                Gender = e.Gender.ToString(),
                Position = e.Position.PositionTitle,
                JobGrade = e.Position.JobGrade.Name,
                StartDate = e.StartDate,
                AnnualLeaveRemaining = annual?.AvailableDays ?? 0,
                LeaveBalances = e.LeaveBalances.Select(lb => new LeaveBalanceSummary
                {
                    LeaveType = lb.LeaveType.Name,
                    AccruedDays = lb.AccruedDays,
                    TakenDays = lb.TakenDays,
                    AvailableDays = lb.AvailableDays
                }).ToList()
            };
        }

        private decimal CalculateYearsOfService(DateOnly startDate)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (startDate > today)
                return 0;

            var totalDays = today.DayNumber - startDate.DayNumber;
            return Math.Round(totalDays / 365.25m, 2);
        }
    }
}