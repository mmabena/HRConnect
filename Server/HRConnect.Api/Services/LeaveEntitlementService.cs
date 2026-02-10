namespace HRConnect.Api.Services
{
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles leave entitlement allocation and recalculation logic.
    /// </summary>
    public class LeaveEntitlementService : ILeaveEntitlementService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IJobGradeRepository _jobGradeRepository;
        private readonly ILeaveTypeRepository _leaveTypeRepository;
        private readonly ILeaveEntitlementRepository _leaveEntitlementRepository;
        private readonly IEmployeeLeaveBalanceRepository _employeeLeaveBalanceRepository;

        public LeaveEntitlementService(
            IEmployeeRepository employeeRepository,
            IJobGradeRepository jobGradeRepository,
            ILeaveTypeRepository leaveTypeRepository,
            ILeaveEntitlementRepository leaveEntitlementRepository,
            IEmployeeLeaveBalanceRepository employeeLeaveBalanceRepository)
        {
            _employeeRepository = employeeRepository;
            _jobGradeRepository = jobGradeRepository;
            _leaveTypeRepository = leaveTypeRepository;
            _leaveEntitlementRepository = leaveEntitlementRepository;
            _employeeLeaveBalanceRepository = employeeLeaveBalanceRepository;
        }

        /// <summary>
        /// Allocates initial leave entitlements when an employee is created.
        /// </summary>
        public async Task AllocateOnEmployeeHireAsync(int employeeId)
        {
            // 1. Load employee
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee == null)
                throw new InvalidOperationException("Employee not found.");

            // 2. Load latest job grade
            var jobGrade = await _jobGradeRepository.GetLatestByEmployeeIdAsync(employeeId);
            if (jobGrade == null)
                throw new InvalidOperationException("Job grade not found.");

            // 3. Calculate years of service
            int yearsOfService = DateTime.UtcNow.Year - employee.DateCreated.Year;

            // =====================
            // ANNUAL LEAVE
            // =====================
            var leaveTypes = await _leaveTypeRepository.GetAllAsync();
            var annualLeave = leaveTypes.FirstOrDefault(l => l.Code == "AL");
            if (annualLeave == null)
                throw new InvalidOperationException("Annual Leave type not found.");

            int annualDaysEntitled = CalculateAnnualLeaveDays(
                jobGrade.JobGradeName,
                yearsOfService);

            var annualEntitlement = new LeaveEntitlementRule
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = annualLeave.LeaveTypeId,
                JobGradeId = jobGrade.JobGradeId,
                EmployeeName = $"{employee.Name} {employee.Surname}",
                JobGrade = jobGrade.JobGradeName,
                YearsOfService = yearsOfService,
                DaysEntitled = annualDaysEntitled,
                Status = "Active"
            };

            await _leaveEntitlementRepository.AddAsync(annualEntitlement);

            var annualBalance = new EmployeeLeaveBalance
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = annualLeave.LeaveTypeId,
                DaysAvailable = annualDaysEntitled,
                LastUpdated = DateTime.UtcNow
            };

            await _employeeLeaveBalanceRepository.AddAsync(annualBalance);

            // =====================
            // SICK LEAVE
            // =====================
            await AllocateSickLeaveAsync(employee, jobGrade, yearsOfService);

            // =============================
            // FAMILY RESPONSIBILITY LEAVE
            // =============================
            await AllocateFamilyResponsibilityLeaveAsync(employee, jobGrade, yearsOfService);

            // =====================
            // MATERNITY LEAVE
            // =====================
            await AllocateMaternityLeaveAsync(employee, jobGrade, yearsOfService);

            // =====================
            // PERSIST ONCE
            // =====================
            await _leaveEntitlementRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Calculates annual leave days based on job grade and years of service.
        /// </summary>
        private static int CalculateAnnualLeaveDays(string jobGrade, int yearsOfService)
        {
            var normalizedJobGrade = jobGrade.Trim();

            bool isUnskilledToMiddle =
                normalizedJobGrade == "Junior Management" ||
                normalizedJobGrade == "Middle Management" ||
                normalizedJobGrade == "Skilled/Semi Skilled" ||
                normalizedJobGrade == "Unskilled";

            bool isSeniorManagement =
                normalizedJobGrade == "Top/Senior Management";

            bool isExecutiveDirector =
                normalizedJobGrade == "Executive Director";

            if (yearsOfService < 3)
            {
                if (isUnskilledToMiddle) return 15;
                if (isSeniorManagement) return 18;
                if (isExecutiveDirector) return 22;
            }

            if (yearsOfService >= 3 && yearsOfService <= 5)
            {
                if (isUnskilledToMiddle) return 18;
                if (isSeniorManagement) return 21;
                if (isExecutiveDirector) return 25;
            }

            if (yearsOfService > 5)
            {
                if (isUnskilledToMiddle) return 20;
                if (isSeniorManagement) return 23;
                if (isExecutiveDirector) return 27;
            }

            return 0;
        }

        /// <summary>
        /// Determines how many sick leave days are accessible based on time employed.
        /// </summary>
        private static int CalculateSickLeaveDays(DateTime employmentDate)
        {
            int monthsEmployed =
                (DateTime.UtcNow.Year - employmentDate.Year) * 12 +
                DateTime.UtcNow.Month - employmentDate.Month;

            if (monthsEmployed < 0)
                monthsEmployed = 0;

            // First 6 months: 1 day per month
            if (monthsEmployed <= 6)
                return monthsEmployed;

            // After 6 months: access to remaining balance (max 30)
            return 30;
        }

        /// <summary>
        /// Allocates sick leave entitlement and balance.
        /// </summary>
        private async Task AllocateSickLeaveAsync(
            Employee employee,
            JobGrade jobGrade,
            int yearsOfService)
        {
            var leaveTypes = await _leaveTypeRepository.GetAllAsync();
            var sickLeave = leaveTypes.FirstOrDefault(l => l.Code == "SL");
            if (sickLeave == null)
                throw new InvalidOperationException("Sick Leave type not found.");

            // Entitlement is always 30 (3-year cycle)
            var entitlement = new LeaveEntitlementRule
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = sickLeave.LeaveTypeId,
                JobGradeId = jobGrade.JobGradeId,
                EmployeeName = $"{employee.Name} {employee.Surname}",
                JobGrade = jobGrade.JobGradeName,
                YearsOfService = yearsOfService,
                DaysEntitled = 30,
                Status = "Active"
            };

            await _leaveEntitlementRepository.AddAsync(entitlement);

            int accessibleDays = CalculateSickLeaveDays(employee.DateCreated);

            var balance = new EmployeeLeaveBalance
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = sickLeave.LeaveTypeId,
                DaysAvailable = accessibleDays,
                LastUpdated = DateTime.UtcNow
            };

            await _employeeLeaveBalanceRepository.AddAsync(balance);
        }
        /// <summary>
        /// Allocates Family Responsibility Leave (FRL).
        /// </summary>
        public async Task AllocateFamilyResponsibilityLeaveAsync(
            Employee employee,
            JobGrade jobGrade,
            int yearsOfService)
        {
            var leaveTypes = await _leaveTypeRepository.GetAllAsync();
            var familyResponsibilityLeave = leaveTypes.FirstOrDefault(l => l.Code == "FRL");
            if (familyResponsibilityLeave == null)
            {
                throw new InvalidOperationException("Family Responsibility Leave type not found");
            }
            //entitlement is always 3
            var entitlement = new LeaveEntitlementRule
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = familyResponsibilityLeave.LeaveTypeId,
                JobGradeId = jobGrade.JobGradeId,
                EmployeeName = $"{employee.Name} {employee.Surname}",
                JobGrade = jobGrade.JobGradeName,
                YearsOfService = yearsOfService,
                DaysEntitled = 3,
                Status = "Active"
            };
            await _leaveEntitlementRepository.AddAsync(entitlement);

            //balance starts at full entitlement (resets on anniversary logic later)
            var balance = new EmployeeLeaveBalance
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = familyResponsibilityLeave.LeaveTypeId,
                DaysAvailable = 3,
                LastUpdated = DateTime.UtcNow
            };
            await _employeeLeaveBalanceRepository.AddAsync(balance);
        }
        /// <summary>
        /// Allocates Maternity Leave (ML) for female employees only.
        /// Fixed 120 days including weekends.
        /// </summary>
        private async Task AllocateMaternityLeaveAsync(
            Employee employee,
            JobGrade jobGrade,
            int yearsOfService)
        {
            if (!employee.Gender.Equals("Female", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            var leaveTypes = await _leaveTypeRepository.GetAllAsync();
            var maternityLeave = leaveTypes.FirstOrDefault(l => l.Code == "ML");
            if (maternityLeave == null)
            {
                throw new InvalidOperationException("Maternity Leave type not found");
            }
            var entitlement = new LeaveEntitlementRule
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = maternityLeave.LeaveTypeId,
                JobGradeId = jobGrade.JobGradeId,
                EmployeeName = $"{employee.Name} {employee.Surname}",
                JobGrade = jobGrade.JobGradeName,
                YearsOfService = yearsOfService,
                DaysEntitled = 120,
                Status = "Active"
            };
            await _leaveEntitlementRepository.AddAsync(entitlement);

            var balance = new EmployeeLeaveBalance
            {
                EmployeeId = employee.EmployeeId,
                LeaveTypeId = maternityLeave.LeaveTypeId,
                DaysAvailable = 120,
                LastUpdated = DateTime.UtcNow
            };
            await _employeeLeaveBalanceRepository.AddAsync(balance);

        }

    }
}
