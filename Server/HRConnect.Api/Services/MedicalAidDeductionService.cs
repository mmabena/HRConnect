namespace HRConnect.Api.Services;

using DTOs.Employee;
using DTOs.Payroll.PayrollDeduction.MedicalAidDeduction;
using HRConnect.Api.Models;
using Interfaces;
using Models.PayrollDeduction;

/// <summary>
/// Service implementation for managing medical aid deductions.
/// </summary>
public class MedicalAidDeductionService : IMedicalAidDeductionService
{
  private readonly IMedicalAidDeductionRepository _medicalAidDeductionRepository;
  private readonly IMedicalOptionRepository _medicalOptionRepository;
  private readonly IPayrollRunService _payrollRunService;
  private readonly IEmployeeService _employeeService;

  public MedicalAidDeductionService(
      IMedicalAidDeductionRepository medicalAidDeductionRepository,
      IMedicalOptionRepository medicalOptionRepository,
      IEmployeeService employeeService, IPayrollRunService payrollRunService)
  {
    _medicalAidDeductionRepository = medicalAidDeductionRepository;
    _medicalOptionRepository = medicalOptionRepository;
    _employeeService = employeeService;
    _payrollRunService = payrollRunService;

  }

  public async Task<MedicalAidDeductionDto> GetMedicalAidDeductionsByEmployeeIdAsync(string employeeId)
  {
    var employeeDeductions = await _medicalAidDeductionRepository
        .GetMedicalAidDeductionsByEmployeeIdAsync(employeeId);

    if (employeeDeductions == null || employeeDeductions.Count == 0)
    {
      throw new KeyNotFoundException($"No medical aid deductions found for employee {employeeId}");
    }

    // Return the first/most recent deduction
    var deduction = employeeDeductions.First();
    return MapToDto(deduction);
  }

  public async Task<IReadOnlyList<MedicalAidDeduction>> GetAllMedicalAidDeductions()
  {
    return await _medicalAidDeductionRepository.GetAllMedicalAidDeductionsAsync();
  }

  public async Task<MedicalAidDeductionDto> AddNewMedicalAidDeductions(
      string employeeId,
      int medicalOptionId,
      CreateMedicalDeductionDto request)
  {
    // Get employee details
    var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
    if (employee == null)
    {
      throw new KeyNotFoundException($"Employee with ID {employeeId} not found");
    }

    // Get medical option details to ensure it exists and get category info
    var medicalOption = await _medicalOptionRepository.GetMedicalOptionByIdAsync(medicalOptionId);
    if (medicalOption == null)
    {
      throw new KeyNotFoundException($"Medical option with ID {medicalOptionId} not found");
    }

    // Create the deduction entity
    var deduction = new MedicalAidDeduction
    {
      // Employee details from employee service
      Name = employee.Name,
      Surname = employee.Surname,
      Branch = employee.Branch.ToString(),
      Salary = employee.MonthlySalary,
      EmployeeStartDate = employee.StartDate.ToDateTime(TimeOnly.MinValue),

      // Medical option details
      MedicalOptionId = medicalOptionId,
      MedicalCategoryId = request.MedicalCategoryId > 0
            ? request.MedicalCategoryId
            : medicalOption.MedicalOptionCategoryId,

      // Dependent counts from request
      PrincipalCount = request.PrincipalCount,
      AdultCount = request.AdultCount,
      ChildrenCount = request.ChildrenCount,

      // Premium amounts from request (already calculated by client from eligible options)
      PrincipalPremium = request.PrincipalPremium,
      SpousePremium = request.SpousePremium,
      ChildPremium = request.ChildPremium,
      TotalDeductionAmount = request.TotalDeductionAmount,

      // Effective date (default to now if not specified)
      EffectiveDate = request.EffectiveDate,//?? DateTime.UtcNow,

      // Set as active by default
      IsActive = true,
      CreatedDate = DateTime.UtcNow
    };

    // Save to repository

    await _payrollRunService.AddRecordToCurrentRunAsync(deduction, employee.EmployeeId);
    await _medicalAidDeductionRepository.AddNewMedicalAidDeductionsAsync(deduction);

    return MapToDto(deduction);
  }

  public async Task<MedicalAidDeductionDto> UpdateDeductionByEmpId(string employeeId)
  {
    throw new NotImplementedException();
  }

  /// <summary>
  /// Maps a MedicalAidDeduction entity to a MedicalAidDeductionDto.
  /// </summary>
  private static MedicalAidDeductionDto MapToDto(MedicalAidDeduction deduction)
  {
    return new MedicalAidDeductionDto
    {
      MedicalAidDeductionId = deduction.Id,
      EmployeeId = deduction.EmployeeId ?? string.Empty,
      Name = deduction.Name,
      Surname = deduction.Surname,
      Branch = deduction.Branch,
      Salary = deduction.Salary,
      EmployeeStartDate = deduction.EmployeeStartDate,
      EffectiveDate = deduction.EffectiveDate,
      MedicalOptionId = deduction.MedicalOptionId,
      MedicalCategoryId = deduction.MedicalCategoryId,
      PrincipalCount = deduction.PrincipalCount,
      AdultCount = deduction.AdultCount,
      ChildrenCount = deduction.ChildrenCount,
      PrincipalPremium = deduction.PrincipalPremium,
      SpousePremium = deduction.SpousePremium,
      ChildPremium = deduction.ChildPremium,
      TotalDeductionAmount = deduction.TotalDeductionAmount,
      CreatedDate = deduction.CreatedDate,
      IsActive = deduction.IsActive
    };
  }
}