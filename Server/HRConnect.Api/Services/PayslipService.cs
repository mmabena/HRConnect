namespace HRConnect.Api.Services
{
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces.Payroll;

    public class PayslipService(
        IPayslipRepository payslipRepository) : IPayslipService
    {
        // =========================================================
        // GET PAYSLIP
        // =========================================================
        public async Task<PayslipDto?> GetPayslipAsync(
            string employeeId,
            int payrollRunId,
            int payrollRunNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                throw new ArgumentException(
                    "Employee ID is required.",
                    nameof(employeeId));
            }

            if (payrollRunId <= 0)
            {
                throw new ArgumentException(
                    "Payroll run ID is invalid.",
                    nameof(payrollRunId));
            }

            if (payrollRunNumber < 1 || payrollRunNumber > 12)
            {
                throw new ArgumentException(
                    "Payroll run number must be between 1 and 12.",
                    nameof(payrollRunNumber));
            }

            // ---------------------------------------------------------
            // Get the payroll run
            // ---------------------------------------------------------
            var payrollRun =
                await payslipRepository.GetPayrollRunAsync(
                    employeeId,
                    payrollRunId,
                    payrollRunNumber,
                    cancellationToken);

            if (payrollRun == null)
            {
                return null;
            }

            // ---------------------------------------------------------
            // Get employee
            // ---------------------------------------------------------
            var employee =
                await payslipRepository.GetEmployeeAsync(
                    employeeId,
                    cancellationToken);

            if (employee == null)
            {
                return null;
            }

            // ---------------------------------------------------------
            // Get employee deductions
            // ---------------------------------------------------------
            var deductions =
                await payslipRepository.GetEmployeeDeductionsAsync(
                    employeeId,
                    payrollRunId,
                    payrollRunNumber,
                    cancellationToken);

            // ---------------------------------------------------------
            // Get company contributions
            // ---------------------------------------------------------
            var companyContributions =
                await payslipRepository.GetCompanyContributionsAsync(
                    employeeId,
                    payrollRunId,
                    payrollRunNumber,
                    cancellationToken);

            // ---------------------------------------------------------
            // Calculate deductions
            // ---------------------------------------------------------
            decimal medicalAidDeduction = deductions
                .Where(x => x.DeductionType == "Medical Aid")
                .Sum(x => x.CalculatedDeductionAmount);

            decimal pensionDeduction = deductions
                .Where(x => x.DeductionType == "Pension")
                .Sum(x => x.CalculatedDeductionAmount);

            decimal uifDeduction = deductions
                .Where(x => x.DeductionType == "UIF")
                .Sum(x => x.CalculatedDeductionAmount);

            decimal taxDeduction = deductions
                .Where(x => x.DeductionType == "PAYE")
                .Sum(x => x.CalculatedDeductionAmount);

            // ---------------------------------------------------------
            // Calculate company contributions
            // ---------------------------------------------------------
            decimal totalCompanyContributions =
                companyContributions.Sum(x =>
                    x.DeathAmount +
                    x.DisabilityAmount);

            // ---------------------------------------------------------
            // Calculate total deductions
            // ---------------------------------------------------------
            decimal totalDeductions =
                medicalAidDeduction +
                pensionDeduction +
                uifDeduction +
                taxDeduction;

            // ---------------------------------------------------------
            // Calculate net salary
            // ---------------------------------------------------------
            decimal netSalary =
                employee.MonthlySalary -
                totalDeductions;

            // ---------------------------------------------------------
            // Return payslip
            // ---------------------------------------------------------
            return new PayslipDto
            {
                EmployeeId = employee.EmployeeId,

                Name = employee.Name,

                Surname = employee.Surname,

                Position =
                    employee.Position?.PositionTitle
                    ?? string.Empty,

                EmploymentStatus =
                    employee.EmploymentStatus.ToString(),

                TaxNumber =
                    employee.TaxNumber
                    ?? string.Empty,

                IdNumber =
                    employee.IdNumber
                    ?? string.Empty,

                ContactNumber =
                    employee.ContactNumber
                    ?? string.Empty,

                PhysicalAddress =
                    employee.PhysicalAddress
                    ?? string.Empty,

                StartDate = employee.StartDate,

                PositionId = employee.PositionId,

                MonthlySalary = employee.MonthlySalary,

                NetSalary = netSalary,

                MedicalAidDeduction =
                    medicalAidDeduction,

                PensionDeduction =
                    pensionDeduction,

                UIFDeduction =
                    uifDeduction,

                TaxDeduction =
                    taxDeduction,

                TotalCompanyContributions =
                    totalCompanyContributions
            };
        }


        // =========================================================
        // GET PAYSLIP HISTORY
        // =========================================================
        public async Task<IEnumerable<PayslipHistoryDto>> GetPayslipHistoryAsync(
            string employeeId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                throw new ArgumentException(
                    "Employee ID is required.",
                    nameof(employeeId));
            }

            // ---------------------------------------------------------
            // Get payroll runs for employee
            // ---------------------------------------------------------
            var payrollRuns =
                await payslipRepository.GetPayslipHistoryAsync(
                    employeeId,
                    cancellationToken);

         
            return payrollRuns.Select(payrollRun =>
            {
                int month =
                    PayrollRunToCalendarMonth(
                        payrollRun.PayrollRunNumber);


                int year;

                if (month >= 4)
                {
                    year = payrollRun.PeriodDate.Year;
                }
                else
                {
                    year = payrollRun.PeriodDate.Year + 1;
                }

                return new PayslipHistoryDto
                {
                    PayrollRunId =
                        payrollRun.PayrollRunId,

                    PayrollRunNumber =
                        payrollRun.PayrollRunNumber,

                    Year = year,

                    Month = month
                };
            });
        }


        // =========================================================
        // PAYROLL RUN NUMBER -> CALENDAR MONTH
        // =========================================================
        private int PayrollRunToCalendarMonth(
            int payrollRunNumber)
        {
            return payrollRunNumber switch
            {
                1 => 4,   // April
                2 => 5,   // May
                3 => 6,   // June
                4 => 7,   // July
                5 => 8,   // August
                6 => 9,   // September
                7 => 10,  // October
                8 => 11,  // November
                9 => 12,  // December
                10 => 1,  // January
                11 => 2,  // February
                12 => 3,  // March

                _ => throw new ArgumentOutOfRangeException(
                    nameof(payrollRunNumber),
                    payrollRunNumber,
                    "Payroll run number must be between 1 and 12.")
            };
        }
    }
}