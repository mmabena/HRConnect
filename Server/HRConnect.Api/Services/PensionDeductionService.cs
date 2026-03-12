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
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Pension;
  using HRConnect.Api.Utils.Pension.ValidationHelpers;
  using Microsoft.EntityFrameworkCore;

  public class PensionDeductionService(IPensionDeductionRepository pensionDeductionRepository,
    IEmployeeRepository employeeRepository, IEmployeePensionEnrollmentRepository employeePensionEnrollmentRepository,
    ApplicationDBContext context) : IPensionDeductionService
  {
    private readonly IPensionDeductionRepository _pensionDeductionRepository = pensionDeductionRepository;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IEmployeePensionEnrollmentRepository _employeePensionEnrollmentRepository = employeePensionEnrollmentRepository;
    private readonly ApplicationDBContext _context = context;
    private static readonly decimal MAX_PENSIONCONTRIBUTION_PERCENTAGE = (decimal)27.5 / 100;
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
      PensionDeduction? employeePensionDeduction = await _pensionDeductionRepository
        .GetByEmployeeIdAsync(pensionDeductionUpdateDto.EmployeeId);

      if (employeePensionDeduction != null)
      {
        employeePensionDeduction.PensionOptionId = pensionDeductionUpdateDto.PensionOptionId ?? employeePensionDeduction.PensionOptionId;
        employeePensionDeduction.VoluntaryContribution = pensionDeductionUpdateDto.VoluntaryContribution
          ?? employeePensionDeduction.VoluntaryContribution;
        //employeePensionDeduction.PayrollRunId = pensionDeductionUpdateDto.PayrollRunId ?? employeePensionDeduction.PayrollRunId;
        employeePensionDeduction.CreatedDate = pensionDeductionUpdateDto.CreatedDate ?? employeePensionDeduction.CreatedDate;
        employeePensionDeduction.IsActive = pensionDeductionUpdateDto.IsActive ?? employeePensionDeduction.IsActive;

        PensionDeduction pensionDeduction = await _pensionDeductionRepository.UpdateAsync(employeePensionDeduction);
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

    private static void ValidateVoluntaryContribution(decimal voluntaryContribution, decimal employeeMonthSalary, decimal pensionOptionPercentage)
    {
      float voluntaryContributionPercentage = (float)Math.Round(voluntaryContribution / employeeMonthSalary, 2);

      if ((voluntaryContributionPercentage + (float)pensionOptionPercentage) > (float)MAX_PENSIONCONTRIBUTION_PERCENTAGE)
      {
        throw new ValidationException("Voluntary Contribution + Monthly Salary Contribution cannot exceed 27.5% of salary");
      }
    }

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
      ValidateVoluntaryContribution((decimal)pensionDeductionAddDto.VoluntaryContribution, existingEmployee.MonthlySalary, pensionOptionPercentage);


      if (existingEmployee != null)
      {
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
          PensionContribution = ValidPensionContribution(existingEmployee.MonthlySalary * pensionOptionPercentage),
          VoluntaryContribution = (decimal)pensionDeductionAddDto.VoluntaryContribution,
          EmailAddress = existingEmployee.Email,
          PhyscialAddress = existingEmployee.PhysicalAddress,
          PayrollRunId = 1,
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
  }
}
