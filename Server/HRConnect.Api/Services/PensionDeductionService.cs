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
    private static readonly decimal MAX_MONTHLYCONTRIBUTION = 29166.66M;

    ///<summary>
    ///Add a new pension deduction entry for an employee.
    ///</summary>
    ///<param name="pensionDeductionAddDto">Pension's Deduction Add Request Data Transfer Object</param>
    ///<returns>
    ///Added pension deduction
    ///</returns
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

    ///<summary>
    ///Get all pension deductions.
    ///</summary>
    ///<returns>
    ///List of all pension deductions
    ///</returns
    public async Task<List<PensionDeductionDto>> GetAllPensionDeductionsAsync()
    {
      List<PensionDeduction> pensionDeductions = await _pensionDeductionRepository.GetAllAsync();
      return pensionDeductions.Select(pd => pd.ToPensionDeductionDTO()).ToList();
    }

    ///<summary>
    ///Get employee pension deduction by employee id.
    ///</summary>
    ///<param name="employeeId">Employee's Id</param>
    ///<returns>
    ///Employee's latest pension deduction
    ///</returns
    public async Task<PensionDeductionDto?> GetEmployeePensionDeductionByIdAsync(string employeeId)
    {
      PensionDeduction? employeePensionDeduction = await _pensionDeductionRepository.GetByEmployeeIdAsync(employeeId)
        ?? throw new NotFoundException("Employee not found");
      return employeePensionDeduction.ToPensionDeductionDTO();
    }

    ///<summary>
    ///Gets all pension deductions for a specific payroll run. 
    ///</summary>
    ///<param name="payrollRunId">Payroll roll run Id</param>
    ///<returns>
    ///List of pension deductions for a specific payroll run
    ///</returns
    public async Task<List<PensionDeductionDto>> GetPensionDeductionsByPayRollRunIdAsync(int payrollRunId)
    {
      List<PensionDeduction> pensionDeductions = await _pensionDeductionRepository.GetByPayRollRunIdAsync(payrollRunId);
      return pensionDeductions.Select(pd => pd.ToPensionDeductionDTO()).ToList();
    }

    ///<summary>
    ///Update pension deduction entry
    ///</summary>
    ///<param name="pensionDeductionUpdateDto">Pension's Deduction Update Request Data Transfer Object</param>
    ///<returns>
    ///Updated pension deduction details
    ///</returns
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
        //employeePensionDeduction.CreatedDate = pensionDeductionUpdateDto.CreatedDate ?? employeePensionDeduction.CreatedDate;
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

    ///<summary>
    ///Validate pension contribution 
    ///</summary>
    ///<returns>
    ///Pension contribution that is not above the maximum allowed monthly contribution limit
    ///</returns
    private static decimal ValidPensionContribution(decimal pensionContribution)
    {
      return (pensionContribution > MAX_MONTHLYCONTRIBUTION) ? MAX_MONTHLYCONTRIBUTION : pensionContribution;
    }

    ///<summary>
    ///Auxilary function get employee information by employee id
    ///</summary>
    ///<param name="employeeId">Employee's Id</param>
    ///<returns>
    ///Employee information for a given employee id
    ///</returns
    private async Task<Employee> GetEmployeeInformationAsync(string employeeId)
    {
      return await _employeeRepository.GetEmployeeByIdAsync(employeeId)
        ?? throw new NotFoundException("Employee not found");
    }

    ///<summary>
    ///Auxilary function to get pension option percentage by pension option id
    ///</summary>
    ///<param name="pensionOptionId">Pension Option Id</param>
    ///<returns>
    ///Pension option percentage for a given pension option id
    ///</returns
    private async Task<decimal> GetEmployeePensionOptionPercentageAsync(int pensionOptionId)
    {
      decimal? employeePensionOption = await _context.PensionOptions.Where(po => po.PensionOptionId == pensionOptionId)
        .Select(po => po.ContributionPercentage).FirstOrDefaultAsync();
      return employeePensionOption ?? throw new NotFoundException("Pension option not found");
    }

    ///<summary>
    ///Auxilary function to get employee pension enrollment by employee id
    ///</summary>
    ///<param name="employeeId">Employee's Id</param>
    ///<returns>
    ///Employee pension enrollment details for a given employee id
    ///</returns
    private async Task<EmployeePensionEnrollment> GetEmployeePensionEnrollmentAsync(string employeeId)
    {
      EmployeePensionEnrollment? employeePensionEnrollment = await _employeePensionEnrollmentRepository
        .GetByEmployeeIdAndLastRunIdAsync(employeeId);
      return employeePensionEnrollment ?? throw new NotFoundException("Employee isn't enrolled into any pension option");
    }

    ///<summary>
    ///Auxilary function to create a new pension deduction entry base of existing employee information and their pension enrollment
    ///</summary>
    ///<param name="pensionDeductionAddDto">Pension's Deduction Add Request Data Transfer Object</param>
    ///<returns>
    ///Added pension deduction
    ///</returns
    private async Task<PensionDeduction?> CreateNewPensionDeductionEntry(PensionDeductionAddDto pensionDeductionAddDto)
    {
      ValidatePensionDeductionDtos.ValidateAddDto(pensionDeductionAddDto);
      Employee existingEmployee = await GetEmployeeInformationAsync(pensionDeductionAddDto.EmployeeId);
      EmployeePensionEnrollment existEmployeesPensionEnrollment = await GetEmployeePensionEnrollmentAsync(pensionDeductionAddDto.EmployeeId);
      if (existingEmployee.PensionOptionId == null)
      {
        throw new InvalidOperationException("Employee has no pension option assigned.");
      }

      decimal pensionOptionPercentage = await GetEmployeePensionOptionPercentageAsync((int)existingEmployee.PensionOptionId);
      ValidatePensionDeductionDtos.ValidateVoluntaryContribution(existEmployeesPensionEnrollment.VoluntaryContribution,
        existingEmployee.MonthlySalary, pensionOptionPercentage);

      if (existingEmployee != null)
      {
        PayrollRun? currentPayrollRunId = await _payrollRunRepository.GetCurrentRunAsync();

        PensionDeduction employeesPensionDeduction = new()
        {
          EmployeeId = existingEmployee.EmployeeId,
          FirstName = existingEmployee.Name,
          LastName = existingEmployee.Surname,
          DateJoinedCompany = existingEmployee.StartDate,
          IdNumber = existingEmployee.IdNumber,
          Passport = existingEmployee.PassportNumber,
          TaxNumber = existingEmployee.TaxNumber,
          PensionableSalary = existingEmployee.MonthlySalary,
          PensionOptionId = (int)existingEmployee.PensionOptionId,
          PendsionCategoryPercentage = pensionOptionPercentage,
          PensionContribution = ValidPensionContribution(Math.Round(existingEmployee.MonthlySalary * (pensionOptionPercentage / 100))),
          VoluntaryContribution = existEmployeesPensionEnrollment.VoluntaryContribution,
          TotalPensionContribution =
            ValidPensionContribution(Math.Round(existingEmployee.MonthlySalary * (pensionOptionPercentage / 100)) +
            (decimal)existEmployeesPensionEnrollment.VoluntaryContribution),
          EmailAddress = existingEmployee.Email,
          PhysicalAddress = existingEmployee.PhysicalAddress,
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

    ///<summary>
    ///Add a new pension deduction entry for an employee.
    ///</summary>
    ///<param name="pensionDeductionAddDto">Pension's Deduction Add Request Data Transfer Object</param>
    ///<returns>
    ///Added pension deduction
    ///</returns
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
        ?? throw new NotFoundException("Employee pension enrollment not found");
      employeePensionEnrollment.PensionOptionId = newPensionOptionId;
      employeePensionEnrollment.VoluntaryContribution = voluntaryContribution;
      EmployeePensionEnrollment? updatedEmployeePensionEnrollment = await _employeePensionEnrollmentRepository.UpdateAsync(employeePensionEnrollment);
      if (updatedEmployeePensionEnrollment != null && updatedEmployeePensionEnrollment.PensionOptionId != newPensionOptionId)
      {
        throw new InvalidOperationException("Failed to update employee's pension enrollment");
      }
    }

    ///<summary>
    ///Rollover pension deductions for all employees
    ///</summary>
    public async Task PensionDeductionRollover()
    {
      List<EmployeePensionEnrollment> employeePensionEnrollments = await _employeePensionEnrollmentRepository.GetEmployeePensionEnrollmentsNotLocked();

      foreach (EmployeePensionEnrollment enrollment in employeePensionEnrollments)
      {
        Employee? employee = await _employeeRepository.GetEmployeeByIdAsync(enrollment.EmployeeId);
        if (employee != null && employee.IsActive)
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
            IdNumber = employee.IdNumber,
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
            PhysicalAddress = employee.PhysicalAddress,
            CreatedDate = enrollment.EffectiveDate,
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
