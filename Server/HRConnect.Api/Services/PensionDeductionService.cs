namespace HRConnect.Api.Services
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs.Payroll.Pension;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Pension;
  using HRConnect.Api.Mappers.Payroll.Pension;
  using HRConnect.Api.Models;
  using HRConnect.Api.Models.Payroll;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Utils.Pension.ValidationHelpers;
  using Microsoft.EntityFrameworkCore;

  public class PensionDeductionService(IPensionDeductionRepository pensionDeductionRepository,
    IEmployeeRepository employeeRepository, IEmployeePensionEnrollmentRepository employeePensionEnrollmentRepository,
    IPayrollRunRepository payrollRunRepository, IPayrollRunService payrollRunService, ApplicationDBContext context) : IPensionDeductionService
  {
    private readonly IPensionDeductionRepository _pensionDeductionRepository = pensionDeductionRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IEmployeePensionEnrollmentRepository _employeePensionEnrollmentRepository = employeePensionEnrollmentRepository;
    private readonly IPayrollRunRepository _payrollRunRepository = payrollRunRepository;
    private readonly IPayrollRunService _payrollRunService = payrollRunService;
    private readonly ApplicationDBContext _context = context;
    //private static readonly decimal MAX_PENSIONCONTRIBUTION_PERCENTAGE = (decimal)27.5 / 100;
    private static readonly decimal MAX_MONTHLYCONTRIBUTION = 29166.66M;
    public async Task<PensionDeductionDto?> AddPensionDeductionAsync(PensionDeductionAddDto pensionDeductionAddDto)
    {
      PensionDeduction? newPensionDeductionEntry = await CreateNewPensionDeductionEntry(pensionDeductionAddDto);
      if (newPensionDeductionEntry != null)
      {
        PensionDeduction employeesPensionDeductionEntry = await _pensionDeductionRepository.AddAsync(newPensionDeductionEntry);
        return employeesPensionDeductionEntry.ToPensionDeductionDTO();
      }
      else
      {
        return null;
      }
    }

    public Task<bool> DeleteEmployeePensionDeductionAsync()
    {
      throw new NotImplementedException();
    }

    public async Task<List<PensionDeductionDto>> GetAllPensionDeductionsAsync()
    {
      List<PensionDeduction> pensionDeductions = await _pensionDeductionRepository.GetAllAsync();
      return pensionDeductions.Select(pd => pd.ToPensionDeductionDTO()).ToList();
    }

    public async Task<PensionDeductionDto?> GetEmployeePensionDeductionByIdAsync(string employeeId)
    {
      PensionDeduction? employeePensionDeduction = await _pensionDeductionRepository.GetByEmployeeIdAsync(employeeId)
        ?? throw new NotFoundException("Employee not found");
      return employeePensionDeduction.ToPensionDeductionDTO();
    }

    public async Task<List<PensionDeductionDto>> GetPensionDeductionsByPayRollRunIdAsync(int payrollRunId)
    {
      List<PensionDeduction> pensionDeductions = await _pensionDeductionRepository.GetByPayRollRunIdAsync(payrollRunId);
      return pensionDeductions.Select(pd => pd.ToPensionDeductionDTO()).ToList();
    }

    public async Task<PensionDeductionDto?> UpdateEmployeePensionDeductionAsync(PensionDeductionUpdateDto pensionDeductionUpdateDto)
    {
      ValidatePensionDeductionDtos.ValidateUpdateDto(pensionDeductionUpdateDto);
      Employee existingEmployee = await GetEmployeeInformationAsync(pensionDeductionUpdateDto.EmployeeId);
      PensionDeduction? employeePensionDeduction = await _pensionDeductionRepository
        .GetByEmployeeIdAndIsNotLockedAsync(pensionDeductionUpdateDto.EmployeeId);

      decimal pensionOptionPercentage = await
        GetEmployeePensionOptionPercentageAsync(pensionDeductionUpdateDto.PensionOptionId ?? (int)existingEmployee.PensionOptionId);
      ValidatePensionDeductionDtos.ValidateVoluntaryContribution((decimal)pensionDeductionUpdateDto.VoluntaryContribution, existingEmployee.MonthlySalary, pensionOptionPercentage);

      if (employeePensionDeduction != null)
      {
        employeePensionDeduction.PensionOptionId = pensionDeductionUpdateDto.PensionOptionId ?? employeePensionDeduction.PensionOptionId;
        employeePensionDeduction.VoluntaryContribution = pensionDeductionUpdateDto.VoluntaryContribution
          ?? employeePensionDeduction.VoluntaryContribution;
        decimal updatedPensionOptionPercentage = pensionDeductionUpdateDto.PensionOptionId.HasValue ?
          await GetEmployeePensionOptionPercentageAsync((int)pensionDeductionUpdateDto.PensionOptionId)
          : employeePensionDeduction.PendsionCategoryPercentage;
        employeePensionDeduction.PensionContribution = ValidPensionContribution(Math.Round(existingEmployee.MonthlySalary * (updatedPensionOptionPercentage / 100)));
        employeePensionDeduction.TotalPensionContribution =
          ValidPensionContribution(employeePensionDeduction.PensionContribution + employeePensionDeduction.VoluntaryContribution);
        employeePensionDeduction.CreatedDate = pensionDeductionUpdateDto.CreatedDate ?? employeePensionDeduction.CreatedDate;
        employeePensionDeduction.IsActive = pensionDeductionUpdateDto.IsActive ?? employeePensionDeduction.IsActive;

        PensionDeduction pensionDeduction = await _pensionDeductionRepository.UpdateAsync(employeePensionDeduction);
        await HandlePensionOptionChange(employeePensionDeduction.EmployeeId, employeePensionDeduction.PensionOptionId, employeePensionDeduction.VoluntaryContribution);
        return pensionDeduction.ToPensionDeductionDTO();
      }
      else
      {
        throw new NotFoundException("Pension deduction was not found");
      }
    }
    private static decimal ValidPensionContribution(decimal pensionContribution)
    {
      return (pensionContribution > MAX_MONTHLYCONTRIBUTION) ? MAX_MONTHLYCONTRIBUTION : pensionContribution;
    }

    /*private static void ValidateVoluntaryContribution(decimal voluntaryContribution, decimal employeeMonthSalary, decimal pensionOptionPercentage)
    {
      float voluntaryContributionPercentage = (float)Math.Round(voluntaryContribution / employeeMonthSalary, 2);

      if ((voluntaryContributionPercentage + (float)pensionOptionPercentage) > (float)MAX_PENSIONCONTRIBUTION_PERCENTAGE)
      {
        throw new ValidationException("Voluntary Contribution + Monthly Salary Contribution cannot exceed 27.5% of salary");
      }
    }*/

    private async Task<Employee> GetEmployeeInformationAsync(string employeeId)
    {
      return await _employeeRepository.GetEmployeeByIdAsync(employeeId)
        ?? throw new NotFoundException("Employee not found");
    }

    private async Task<decimal> GetEmployeePensionOptionPercentageAsync(int pensionOptionId)
    {
      decimal? employeePensionOption = await _context.PensionOptions.Where(po => po.PensionOptionId == pensionOptionId)
        .Select(po => po.ContributionPercentage).FirstOrDefaultAsync();
      return employeePensionOption ?? throw new NotFoundException("Pension option not found");
    }

    private async Task<EmployeePensionEnrollment> GetEmployeePensionEnrollmentAsync(string employeeId)
    {
      EmployeePensionEnrollment? employeePensionEnrollment = await _employeePensionEnrollmentRepository.GetByEmployeeIdAsync(employeeId);
      return employeePensionEnrollment ?? throw new NotFoundException("Employee isn't enrolled into any pension option");
    }

    private async Task<PensionDeduction?> CreateNewPensionDeductionEntry(PensionDeductionAddDto pensionDeductionAddDto)
    {
      ValidatePensionDeductionDtos.ValidateAddDto(pensionDeductionAddDto);
      Employee existingEmployee = await GetEmployeeInformationAsync(pensionDeductionAddDto.EmployeeId);
      EmployeePensionEnrollment existEmployeesPensionEnrollment = await GetEmployeePensionEnrollmentAsync(pensionDeductionAddDto.EmployeeId);
      decimal pensionOptionPercentage = await GetEmployeePensionOptionPercentageAsync((int)existingEmployee.PensionOptionId);
      ValidatePensionDeductionDtos.ValidateVoluntaryContribution((decimal)pensionDeductionAddDto.VoluntaryContribution, existingEmployee.MonthlySalary, pensionOptionPercentage);

      if (existingEmployee != null)
      {
        PayrollRun? currentPayrollRunId = await _payrollRunRepository.GetCurrentRunAsync();

        PensionDeduction employeesPensionDeduction = new()
        {
          EmployeeId = existingEmployee.EmployeeId,
          FirstName = existingEmployee.Name,
          LastName = existingEmployee.Surname,
          DateJoinedCompany = existingEmployee.StartDate,
          IDNumber = existingEmployee.IdNumber,
          Passport = existingEmployee.PassportNumber,
          TaxNumber = existingEmployee.TaxNumber,
          PensionableSalary = existingEmployee.MonthlySalary,
          PensionOptionId = (int)existingEmployee.PensionOptionId,
          PendsionCategoryPercentage = pensionOptionPercentage,
          PensionContribution = ValidPensionContribution(Math.Round(existingEmployee.MonthlySalary * (pensionOptionPercentage / 100))),
          VoluntaryContribution = (decimal)pensionDeductionAddDto.VoluntaryContribution,
          TotalPensionContribution =
            ValidPensionContribution(Math.Round(existingEmployee.MonthlySalary * (pensionOptionPercentage / 100)) +
            (decimal)pensionDeductionAddDto.VoluntaryContribution),
          EmailAddress = existingEmployee.Email,
          PhyscialAddress = existingEmployee.PhysicalAddress,
          PayrollRunId = currentPayrollRunId.PayrollRunId,
          CreatedDate = existEmployeesPensionEnrollment.EffectiveDate,
          IsActive = true
        };

        return employeesPensionDeduction;
      }
      else
      {
        return null;
      }
    }

    private async Task HandlePensionOptionChange(string employeeId, int newPensionOptionId, decimal voluntaryContribution)
    {
      Employee employeeNeedingAnUpdate = _employeeRepository.GetEmployeeByIdAsync(employeeId).Result ?? throw new NotFoundException("Employee not found");
      employeeNeedingAnUpdate.PensionOptionId = newPensionOptionId;
      Employee? updatedEmployee = await _employeeRepository.UpdateEmployeeAsync(employeeNeedingAnUpdate);
      if (updatedEmployee != null && updatedEmployee.PensionOptionId != newPensionOptionId)
      {
        throw new InvalidOperationException("Failed to update employee's pension option");
      }

      EmployeePensionEnrollment? employeePensionEnrollment = await _employeePensionEnrollmentRepository.GetByEmployeeIdAndIsNotLockedAsync(employeeId)
        ?? throw new NotFoundException("Employee pension enrollment not found"); ;
      employeePensionEnrollment.PensionOptionId = newPensionOptionId;
      employeePensionEnrollment.VoluntaryContribution = voluntaryContribution;
      EmployeePensionEnrollment? updatedEmployeePensionEnrollment = await _employeePensionEnrollmentRepository.UpdateAsync(employeePensionEnrollment);
      if (updatedEmployeePensionEnrollment != null && updatedEmployeePensionEnrollment.PensionOptionId != newPensionOptionId)
      {
        throw new InvalidOperationException("Failed to update employee's pension enrollment");
      }
    }

    public async Task PensionDeductionRollover()
    {
      List<EmployeePensionEnrollment> employeePensionEnrollments = await _employeePensionEnrollmentRepository.GetEmployeePensionEnrollmentsNotLocked();

      foreach (EmployeePensionEnrollment enrollment in employeePensionEnrollments)
      {
        Employee? employee = await _employeeRepository.GetEmployeeByIdAsync(enrollment.EmployeeId);
        if (employee != null)
        {
          decimal pensionCategoryPercentage = await _context.PensionOptions
          .Where(po => po.PensionOptionId == employee.PensionOptionId)
          .Select(po => po.ContributionPercentage).FirstOrDefaultAsync();

          PensionDeduction pensionDeduction = new()
          {
            EmployeeId = enrollment.EmployeeId,
            FirstName = employee.Name,
            LastName = employee.Surname,
            DateJoinedCompany = employee.StartDate,
            IDNumber = employee.IdNumber,
            Passport = employee.PassportNumber,
            TaxNumber = employee.TaxNumber,
            PensionableSalary = employee.MonthlySalary,
            PensionOptionId = enrollment.PensionOptionId,
            PendsionCategoryPercentage = pensionCategoryPercentage,
            PensionContribution = Math.Round(employee.MonthlySalary * (pensionCategoryPercentage / 100)),
            VoluntaryContribution = enrollment.VoluntaryContribution,
            TotalPensionContribution =
              ValidPensionContribution(Math.Round(employee.MonthlySalary * (pensionCategoryPercentage / 100)) + enrollment.VoluntaryContribution),
            EmailAddress = employee.Email,
            PhyscialAddress = employee.PhysicalAddress,
            CreatedDate = DateOnly.FromDateTime(DateTime.Now),
            PayrollRunId = enrollment.PayrollRunId,
            IsActive = true
          };

          PensionDeduction? existingEmployeePensionDeduction = await _pensionDeductionRepository
            .GetByEmployeeIdAndLastRunIdAsync(pensionDeduction.EmployeeId, pensionDeduction.PayrollRunId);

          if (existingEmployeePensionDeduction == null)
          {
            await _payrollRunService.AddRecordToCurrentRunAsync(pensionDeduction, enrollment.EmployeeId);
            _ = await _pensionDeductionRepository.AddAsync(pensionDeduction);
          }
        }
      }
    }
  }
}
