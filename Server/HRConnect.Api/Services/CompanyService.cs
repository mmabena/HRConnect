namespace HRConnect.Api.Services
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.Models;
  using System.Globalization;
  using HRConnect.Api.DTOs.Company;
  using System.IO;
  using Microsoft.AspNetCore.SignalR;
  using HRConnect.Api.Data;
  using HRConnect.Api.Hubs;
  using HRConnect.Api.Mappers;
  using Microsoft.EntityFrameworkCore;
  using HRConnect.Api.Interfaces;
  public class CompanyService : ICompanyService
  {
    private readonly ApplicationDBContext _context;
    private readonly ICompanyRepository _companyRepo;
    private readonly IHubContext<CompanyHub> _companyHubContext;

    public CompanyService(ApplicationDBContext context, ICompanyRepository companyRepo, IHubContext<CompanyHub> companyHubContext)
    {
      _context = context;
      _companyRepo = companyRepo;
      _companyHubContext = companyHubContext;
    }
    /// <summary>
    /// Retrieves all companies from the system.
    /// </summary>
    /// <returns>A list of CompanyDto objects</returns>
    public async Task<List<CompanyDto>> GetAllCompaniesAsync()
    {
      var companies = await _companyRepo.GetAllCompaniesAsync();
      var result = new List<CompanyDto>();

      foreach (var company in companies)
      {
        var employeeCount = await _context.Employees
          .CountAsync(e => e.CompanyId == company.CompanyId);

        result.Add(new CompanyDto
        {
          CompanyId = company.CompanyId,
          CompanyName = company.CompanyName,
          RegistrationNumber = company.RegistrationNumber,
          UIFNumber = company.UIFNumber,
          VATNumber = company.VATNumber,
          ContactNumber = company.ContactNumber,
          CompanyAddress = company.CompanyAddress,
          EmployeeCount = employeeCount
        });
      }
      return result;
    }
    /// <summary>
    /// Retrieves a single company by its Company ID.
    /// </summary>
    /// <param name="companyId">The company identifier</param>
    /// <returns>The CompanyDto object if found, null otherwise</returns>
    public async Task<CompanyDto?> GetCompanyByIdAsync(string companyId)
    {
      var company = await _companyRepo.GetCompanyByIdAsync(companyId);
      return company?.ToCompanyDto();
    }
    /// <summary>
    /// Creates a new company in the system.
    /// Performs validation and generates a unique Company ID.
    /// </summary>
    /// <param name="companyRequestDto">The company DTO containing creation details</param>
    /// <returns>The created CompanyDto object</returns>
    public async Task<CompanyDto> CreateCompanyAsync(CreateCompanyRequestDto companyRequestDto)
    {
      await ValidateCreate(companyRequestDto);
      companyRequestDto.CompanyId = await GenerateCompanyId(companyRequestDto.CompanyName);

      var new_company = companyRequestDto.ToCompanyFromCreateDTO();

      var createdCompany = await _companyRepo.CreateCompanyAsync(new_company);

      await _companyHubContext.Clients.All.SendAsync(
        "CompanyCreated",
        new
        {
          CompanyId = companyRequestDto.CompanyId,
          CompanyName = companyRequestDto.CompanyId,
          RegistrationNumber = companyRequestDto.CompanyId,
          UIFNumber = companyRequestDto.CompanyId,
          VATNumber = companyRequestDto.CompanyId,
          ContactNumber = companyRequestDto.CompanyId,
          CompanyAddress = companyRequestDto.CompanyId
        }
      );

      return createdCompany.ToCompanyDto(); 
    }
    /// <summary>
    /// Generates a unique Company ID based on the company name prefix.
    /// </summary>
    /// <param name="companyName">The company name used to generate the prefix</param>
    /// <returns>A unique Company ID</returns>
    private async Task<string> GenerateCompanyId(string companyName)
    {
      string prefix = companyName.Length >= 3
          ? companyName.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture)
          : companyName.ToUpper(CultureInfo.InvariantCulture).PadRight(3, 'X');
      int nextNum = 1;

      var existingIds = await _companyRepo.GetAllCompanyIdsWithPrefix(prefix);
      if (existingIds.Count > 0)
      {
        var maxNum = existingIds
                .Select(id => int.Parse(id.AsSpan(3), CultureInfo.InvariantCulture))
                .Max();

        nextNum = maxNum + 1;
      }
      return $"{prefix}{nextNum:D3}";
    }
    /// <summary>
    /// Validates the company creation request.
    /// Ensures required fields are present and values are unique.
    /// </summary>
    /// <param name="companyRequestDto">The company creation DTO</param>
    /// <exception cref="ArgumentException">Thrown when input validation fails</exception>
    /// <exception cref="InvalidOperationException">Thrown when duplicate records are detected</exception>
    private async Task ValidateCreate(CreateCompanyRequestDto companyRequestDto)
    {
      if (string.IsNullOrWhiteSpace(companyRequestDto.CompanyName))
        throw new ValidationException("Company name is required");

      if (string.IsNullOrWhiteSpace(companyRequestDto.CompanyAddress))
        throw new ValidationException("Company address is required");

      if (companyRequestDto.RegistrationNumber.Length != 14)
        throw new ValidationException("Registration number must be 14 digits");

      if (companyRequestDto.UIFNumber.Length != 10)
        throw new ValidationException("UIF number must be 10 digits");

      if (companyRequestDto.ContactNumber.Length != 10)
        throw new ValidationException("Contact number must be 10 digits");

      if (!string.IsNullOrWhiteSpace(companyRequestDto.RegistrationNumber) &&
          await _companyRepo.GetCompanyByRegNumberAsync(companyRequestDto.RegistrationNumber) != null)
        throw new BusinessRuleException("A company with the same registration number already exists");

      if (!string.IsNullOrWhiteSpace(companyRequestDto.UIFNumber) &&
          await _companyRepo.GetCompanyByUIFAsync(companyRequestDto.UIFNumber) != null)
        throw new BusinessRuleException("A company with the same UIF number already exists");

      if (!string.IsNullOrWhiteSpace(companyRequestDto.VATNumber) &&
          await _companyRepo.GetCompanyByVATAsync(companyRequestDto.VATNumber) != null)
        throw new BusinessRuleException("A company with the same VAT number already exists");

      if (!string.IsNullOrWhiteSpace(companyRequestDto.CompanyName) &&
          await _companyRepo.GetCompanyByNameAsync(companyRequestDto.CompanyName) != null)
        throw new BusinessRuleException("A company with the same company name already exists");

      if (!string.IsNullOrWhiteSpace(companyRequestDto.ContactNumber) &&
          await _companyRepo.GetCompanyByContactNumberAsync(companyRequestDto.ContactNumber) != null)
        throw new BusinessRuleException("A company with the same contact number already exists");
    }
  }
}