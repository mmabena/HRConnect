namespace HRConnect.Api.Services
{
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces.Payroll;

    public class PayslipService(
        IPayslipRepository payslipRepository) : IPayslipService
    {
  
        public async Task<PayslipDto?> GetPayslipAsync(
            string employeeId,
            int payrollRunId,
            int payrollRunNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException(
                    "Employee ID is required.",
                    nameof(employeeId));

            if (payrollRunId <= 0)
                throw new ArgumentException(
                    "Payroll run ID is invalid.",
                    nameof(payrollRunId));

            if (payrollRunNumber < 1 || payrollRunNumber > 12)
                throw new ArgumentException(
                    "Payroll run number must be between 1 and 12.",
                    nameof(payrollRunNumber));

            var payrollRun =
                await payslipRepository.GetPayrollRunAsync(
                    employeeId,
                    payrollRunId,
                    payrollRunNumber,
                    cancellationToken);

            if (payrollRun == null)
                return null;


            var employee =
                await payslipRepository.GetEmployeeAsync(
                    employeeId,
                    cancellationToken);

            if (employee == null)
                return null;


            var deductions =
                await payslipRepository.GetEmployeeDeductionsAsync(
                    employeeId,
                    payrollRunId,
                    payrollRunNumber,
                    cancellationToken);


            var medicalAidDeductions =
                await payslipRepository.GetMedicalAidDeductionsAsync(
                    employeeId,
                    payrollRunId,
                    payrollRunNumber,
                    cancellationToken);

            var pensionFund =
                await payslipRepository.GetPensionFundAsync(
                    employeeId,
                    cancellationToken);

            var companyContributions =
                await payslipRepository.GetCompanyContributionsAsync(
                    employeeId,
                    payrollRunId,
                    payrollRunNumber,
                    cancellationToken);

            decimal medicalAidDeduction =
                medicalAidDeductions.Sum(
                    x => x.TotalDeductionAmount);

  
            decimal pensionDeduction =
                pensionFund == null
                    ? 0
                    : employee.MonthlySalary *
                      (pensionFund.ContributionPercentage / 100m);

  
            decimal uifDeduction =
                deductions
                    .Where(x => x.DeductionType == "UIF")
                    .Sum(x => x.CalculatedDeductionAmount);

 
            decimal taxDeduction =
                deductions
                    .Where(x => x.DeductionType == "PAYE")
                    .Sum(x => x.CalculatedDeductionAmount);

            decimal totalCompanyContributions =
                companyContributions.Sum(x =>
                    x.BEEAmount +
                    x.DeathAmount +
                    x.DisabilityAmount);

            decimal totalDeductions =
                medicalAidDeduction +
                pensionDeduction +
                uifDeduction +
                taxDeduction;


            decimal netSalary =
                employee.MonthlySalary -
                totalDeductions;

   
            return new PayslipDto
            {
                EmployeeId =
                    employee.EmployeeId,

                Name =
                    employee.Name,

                Surname =
                    employee.Surname,

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

                StartDate =
                    employee.StartDate,

                PositionId =
                    employee.PositionId,

                MonthlySalary =
                    employee.MonthlySalary,

                NetSalary =
                    netSalary,

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

        public async Task<PayslipViewDto?> GetPayslipViewAsync(
            string employeeId,
            int payrollRunId,
            int payrollRunNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException(
                    "Employee ID is required.",
                    nameof(employeeId));

            if (payrollRunId <= 0)
                throw new ArgumentException(
                    "Payroll run ID is invalid.",
                    nameof(payrollRunId));

            if (payrollRunNumber < 1 || payrollRunNumber > 12)
                throw new ArgumentException(
                    "Payroll run number must be between 1 and 12.",
                    nameof(payrollRunNumber));

            var payrollRun =
                await payslipRepository.GetPayrollRunAsync(
                    employeeId,
                    payrollRunId,
                    payrollRunNumber,
                    cancellationToken);

            if (payrollRun == null)
                return null;


            var employee =
                await payslipRepository.GetEmployeeAsync(
                    employeeId,
                    cancellationToken);

            if (employee == null)
                return null;

            var deductions =
                await payslipRepository.GetEmployeeDeductionsAsync(
                    employeeId,
                    payrollRunId,
                    payrollRunNumber,
                    cancellationToken);

            var medicalAidDeductions =
                await payslipRepository.GetMedicalAidDeductionsAsync(
                    employeeId,
                    payrollRunId,
                    payrollRunNumber,
                    cancellationToken);


            var pensionFund =
                await payslipRepository.GetPensionFundAsync(
                    employeeId,
                    cancellationToken);

            var companyContributions =
                await payslipRepository.GetCompanyContributionsAsync(
                    employeeId,
                    payrollRunId,
                    payrollRunNumber,
                    cancellationToken);

            decimal medicalAidDeduction =
                medicalAidDeductions.Sum(
                    x => x.TotalDeductionAmount);
  
            decimal uifDeduction =
                deductions
                    .Where(x => x.DeductionType == "UIF")
                    .Sum(x => x.CalculatedDeductionAmount);

            decimal taxDeduction =
                deductions
                    .Where(x => x.DeductionType == "PAYE")
                    .Sum(x => x.CalculatedDeductionAmount);
        
            decimal pensionDeduction =
                pensionFund == null
                    ? 0
                    : employee.MonthlySalary *
                      (pensionFund.ContributionPercentage / 100m);

     
            decimal totalDeductions =
                medicalAidDeduction +
                pensionDeduction +
                uifDeduction +
                taxDeduction;

  
            decimal netSalary =
                employee.MonthlySalary -
                totalDeductions;


            decimal totalCompanyContributions =
                companyContributions.Sum(
                    x =>
                        x.DeathAmount +
                        x.DisabilityAmount);

            return new PayslipViewDto
            {
                MonthlySalary =
                    employee.MonthlySalary,

                MedicalAidDeduction =
                    medicalAidDeduction,

                PensionDeduction =
                    pensionDeduction,

                UIFDeduction =
                    uifDeduction,

                TaxDeduction =
                    taxDeduction,

                TotalDeductions =
                    totalDeductions,

                NetSalary =
                    netSalary,

                TotalCompanyContributions =
                    totalCompanyContributions
            };
        }


        public async Task<IEnumerable<PayslipHistoryDto>>
            GetPayslipHistoryAsync(
                string employeeId,
                CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                throw new ArgumentException(
                    "Employee ID is required.",
                    nameof(employeeId));


            var payrollRuns =
                await payslipRepository.GetPayslipHistoryAsync(
                    employeeId,
                    cancellationToken);

            var payslipHistory =
                payrollRuns
                    .Select(payrollRun =>
                    {
                        int month =
                            PayrollRunToCalendarMonth(
                                payrollRun.PayrollRunNumber);


                        int year;

                        if (month >= 4)
                        {
                            year =
                                payrollRun.PeriodDate.Year;
                        }
                        else
                        {
                            year =
                                payrollRun.PeriodDate.Year + 1;
                        }

                        return new PayslipHistoryDto
                        {
                            PayrollRunId =
                                payrollRun.PayrollRunId,

                            PayrollRunNumber =
                                payrollRun.PayrollRunNumber,

                            Year =
                                year,

                            Month =
                                month
                        };
                    })

                    .GroupBy(x => new
                    {
                        x.Year,
                        x.Month,
                        x.PayrollRunNumber
                    })

                  
                    .Select(group =>
                        group
                            .OrderByDescending(
                                x => x.PayrollRunId)
                            .First())

              
                    .OrderByDescending(
                        x => x.Year)

                    .ThenByDescending(
                        x => x.Month)

                    .ThenByDescending(
                        x => x.PayrollRunNumber)

                    .ToList();

            return payslipHistory;
        }

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