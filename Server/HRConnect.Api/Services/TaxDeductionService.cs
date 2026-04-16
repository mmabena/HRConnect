namespace HRConnect.Api.Services
{
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Data;
  using HRConnect.Api.Models;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Repositories;
  using Microsoft.AspNetCore.Http;
  using Microsoft.EntityFrameworkCore;
  using OfficeOpenXml;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Threading.Tasks;
  using System.ComponentModel;
  using System.Linq.Expressions;
  using System.Data.Common;
  using HRConnect.Api.Mappers;
  using HRConnect.Api.DTOs.TaxDeduction;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Models.Payroll;


  /// <summary>
  /// This service is responsible for handling tax deduction operations which includes:
  /// calculations of tax based on remuneration and age,
  /// retrieval, update of tax deduction data, and 
  /// upload/validation of Excel tax tables.
  /// </summary>
  public class TaxDeductionService : ITaxDeductionService
  {
    private readonly ITaxDeductionRepository _repository;
    /// <summary>
    /// Initializes a new instance of <see cref="TaxDeductionService"/> with the specified repository.
    /// </summary>
    /// <param name="repository">this is the repository instance for tax deductions</param>
    /// <param name="context">this is the application database context</param>
    public TaxDeductionService(ITaxDeductionRepository repository)
    {
      _repository = repository;
    }

    /// <summary>
    /// Calculates the tax payable based on the 
    /// tax year(which is automatic based on the active tax table), remuneration, and age.
    /// Matches the remuneration to the correct tax bracket based on the upper bound.
    /// </summary>
    /// <param name="remuneration">Employee's salary</param>
    /// <param name="age">Employee's age</param>
    /// <returns>The tax amount applicable for the given parameter</returns>
    public async Task<decimal> CalculateTaxAsync(decimal remuneration, int age)
    {
      var today = DateTime.UtcNow.Date;

      // Find the active tax table for today
      var activeUploads = await _repository.GetActiveTaxTableUploadsAsync();
      var activeUpload = activeUploads
          .OrderByDescending(x => x.EffectiveFrom)
          .FirstOrDefault(x => x.EffectiveFrom <= today &&
                               (x.EffectiveTo == null || x.EffectiveTo >= today));

      if (activeUpload == null)
      {
        throw new ArgumentException("No active tax table found for the current date.");
      }

      int taxYear = activeUpload.TaxYear;

      // Try to find a tax row in the table
      var allDeductions = await _repository.GetTaxDeductionsByYearAsync(taxYear);
      var taxRow = allDeductions
                .Where(x => remuneration <= x.Remuneration)
                .OrderBy(x => x.Remuneration)
                .FirstOrDefault();

      if (taxRow != null)
      {
        //calculation
        return age switch
        {
          <= 64 => taxRow.TaxUnder65,
          <= 74 => taxRow.Tax65To74,
          _ => taxRow.TaxOver75
        };
      }
      else
      {

        decimal baseAmount = age switch
        {
          <= 64 => 54481m,
          <= 74 => 53694m,
          _ => 53432m
        };

        decimal excess = Math.Max(0, remuneration - 156_328m / 12);
        decimal tax = baseAmount + (0.45m * excess);

        // Disregard cents (round down)
        return Math.Floor(tax);
      }
    }

    /// <summary>
    /// Retrieves all tax deductions for the tax year
    /// </summary>
    /// <param name="taxYear">The year to retrieve deductions for</param>
    /// <returns>List of tax deductions as DTOs</returns>
    public async Task<List<TaxDeductionDto>> GetAllTaxDeductionsAsync(int taxYear)
    {
      var entities = await _repository.GetTaxDeductionsByYearAsync(taxYear);
      var ordered = entities.OrderBy(x => x.Remuneration).ToList();
      return ordered.Select(TaxDeductionMapper.ToDto).ToList();
    }


    /// <summary>
    /// Updates a single tax deduction row with new values.
    /// </summary>
    /// <param name="dto">DTO containing updated tax deduction information.</param>
    /// <exception cref="ArgumentException">Thrown when the tax deduction row does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown if attempting to change the TaxYear.</exception>
    public async Task UpdateTaxDeductionAsync(UpdateTaxDeductionDto dto)
    {
      var deductions = await _repository.GetTaxDeductionsByYearAsync(dto.TaxYear);
      var entity = deductions.FirstOrDefault(x => x.Id == dto.Id);
      if (entity == null)
      {
        throw new ArgumentException("Tax deduction not found.");
      }

      if (entity.TaxYear != dto.TaxYear)
      {
        throw new InvalidOperationException("Cannot change TaxYear.");
      }

      entity.Remuneration = dto.Remuneration;
      entity.AnnualEquivalent = dto.AnnualEquivalent;
      entity.TaxUnder65 = dto.TaxUnder65;
      entity.Tax65To74 = dto.Tax65To74;
      entity.TaxOver75 = dto.TaxOver75;

      await _repository.SaveChangesAsync();
    }

    /// <summary>
    /// Generates the final tax deduction for an employee based on their 
    /// remuneration, age, pension contributions, and medical credits.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<FinalTaxDeduction> GenerateTaxAsync(
      TaxCalculationDto request,
      string email)
    {

      var employee = await _repository.GetEmployeeByEmailAsync(email);
      if (employee == null)
      {
        throw new KeyNotFoundException("Employee not found");
      }

      var payrollRun = await _repository.GetActivePayrollRunAsync();
      if (payrollRun == null)
      {
        throw new KeyNotFoundException("No active payroll run");
      }
      if (payrollRun.IsFinalised)
      {
        throw new InvalidOperationException("Payroll month is finalised. Recalculation is blocked.");
      }

      if (payrollRun.IsFinalised)
      {
        throw new InvalidOperationException("Payroll run has been finalised. Recalculation is blocked.");
      }

      var existing = await _repository.GetExistingFinalTaxAsync(employee.EmployeeId,
        payrollRun.PayrollRunId);


      if (existing?.IsLocked == true)
      {
        throw new InvalidOperationException("Payroll is locked");
      }

      var pension = await _repository.GetPensionByEmployeeIdAsync(employee.EmployeeId);
      if (pension == null)
      {
        throw new KeyNotFoundException("Pension record not found");
      }

      var today = DateOnly.FromDateTime(DateTime.Today);

      int age = today.Year - employee.DateOfBirth.Year;

      if (employee.DateOfBirth > today.AddYears(-age))
        age--;

      decimal monthlySalary = employee.MonthlySalary;

      decimal pensionContribution = pension.TotalPensionContribution;

      decimal pensionableIncome = monthlySalary - pensionContribution;

      decimal taxBeforeCredits =
          await CalculateTaxAsync(pensionableIncome, age);

      bool hasMedicalAid = request.MedicalAidMembers > 0
                        || request.MedicalAidDependants > 0
                        || request.MedicalAidChildren > 0;

      decimal medicalCredit = hasMedicalAid
          ? 364m +
            (Math.Max(0, request.MedicalAidMembers - 1) * 364m) +
            (request.MedicalAidDependants * 364m) +
            (request.MedicalAidChildren * 246m)
          : 0m;

      decimal finalTax = Math.Max(0, taxBeforeCredits - medicalCredit);

      decimal uifEmployee = CalculateUifEmployee(monthlySalary);
      decimal uifEmployer = CalculateUifEmployer(monthlySalary);
      decimal sdl = CalculateSdl(monthlySalary);

      decimal netSalary = monthlySalary - pensionContribution - finalTax - uifEmployee;

      int taxYear = DateTime.Now.Year;

      var record = new FinalTaxDeduction
      {
        PayrollRunId = payrollRun.PayrollRunId,
        EmployeeId = employee.EmployeeId,

        Name = employee.Name,
        Surname = employee.Surname,
        IdNumber = employee.IdNumber,
        PassportNumber = employee.PassportNumber,

        TaxYear = taxYear,

        MonthlySalary = monthlySalary,   // ✅ gross
        PensionableIncome = pensionableIncome,
        PensionContribution = pensionContribution,

        MedicalAidMembers = request.MedicalAidMembers,
        MedicalAidDependants = request.MedicalAidDependants,
        MedicalAidChildren = request.MedicalAidChildren,
        MedicalTaxCredit = medicalCredit,

        TaxDeductionAmount = finalTax,
        UifEmployeeAmount = uifEmployee,
        UifEmployerAmount = uifEmployer,
        SdlAmount = sdl,
        NetSalary = netSalary,

        TaxCode = GenerateTaxCode(
              employee.EmployeeId,
              payrollRun.PayrollRunId,
              taxYear),

        IsLocked = false
      };

      await _repository.AddFinalTaxDeductionAsync(record);
      await _repository.SaveChangesAsync();

      return record;
    }

    /// <summary>
    /// Generates a unique tax code based on employee ID, payroll run ID, and tax year.
    /// </summary>
    /// <param name="employeeId"></param>
    /// <param name="payrollRunId"></param>
    /// <param name="taxYear"></param>
    /// <returns></returns>
    private string GenerateTaxCode(string employeeId, int payrollRunId, int taxYear)
    {
      return $"TX-{taxYear}-{payrollRunId}-{employeeId}";
    }

    /// <summary>
    /// Calculates the UIF contributions for both employee and employer, 
    /// as well as the SDL amount based on the monthly salary.
    /// </summary>
    private const decimal UifRate = 0.01m;
    private const decimal UifCap = 177.12m;
    private const decimal SdlRate = 0.01m;

    /// <summary>
    /// Calculates the UIF employee contribution based on the monthly salary,
    /// ensuring it does not exceed the UIF cap.
    /// </summary>
    /// <param name="monthlySalary"></param>
    /// <returns></returns>
    private decimal CalculateUifEmployee(decimal monthlySalary)
    {
      if (monthlySalary <= 0) return 0;
      decimal contribution = monthlySalary * UifRate;
      return contribution > UifCap ? UifCap : contribution;
    }

    /// <summary>
    /// Calculates the UIF employer contribution based on the monthly salary,
    /// ensuring it does not exceed the UIF cap.
    /// </summary>
    /// <param name="monthlySalary"></param>
    /// <returns></returns>
    private decimal CalculateUifEmployer(decimal monthlySalary)
    {
      if (monthlySalary <= 0) return 0;
      decimal contribution = monthlySalary * UifRate;
      return contribution > UifCap ? UifCap : contribution;
    }

    /// <summary>
    /// Calculates the SDL amount based on the monthly salary.
    /// </summary>
    /// <param name="monthlySalary"></param>
    /// <returns></returns>
    private decimal CalculateSdl(decimal monthlySalary)
    {
      if (monthlySalary <= 0) return 0;
      return monthlySalary * SdlRate;
    }
  }
}
