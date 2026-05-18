namespace HRConnect.Api.Repository
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Data;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Storage;
  public class CompanyRepository : ICompanyRepository
  {
    private readonly ApplicationDBContext _context;
    public CompanyRepository(ApplicationDBContext context)
    {
      _context = context;
    }
    /// <summary>
    /// Retrieves all companies from the database.
    /// </summary>
    /// <returns>A list of all Company entities.</returns>
    public async Task<List<Company>> GetAllCompaniesAsync()
    {
      return await _context.Companies
              .ToListAsync();
    }
    /// <summary>
    /// Retrieves a company by its unique Company ID.
    /// </summary>
    /// <param name="companyId">The company identifier.</param>
    /// <returns>The matching Company entity, or null if not found.</returns>
    public async Task<Company?> GetCompanyByIdAsync(string companyId)
    {
      return await _context.Companies
              .FirstOrDefaultAsync(c => c.CompanyId == companyId);
    }
    /// <summary>
    /// Creates a new company record in the database.
    /// </summary>
    /// <param name="companyModel">The Company entity.</param>
    /// <returns>The created Company entity.</returns>
    public async Task<Company> CreateCompanyAsync(Company companyModel)
    {
      await _context.Companies.AddAsync(companyModel);
      await _context.SaveChangesAsync();
      return companyModel;
    }
    /// <summary>
    /// Retrieves company IDs that start with the specified prefix.
    /// Used for company ID generation and uniqueness validation.
    /// </summary>
    /// <param name="prefix">The company ID prefix to search for.</param>
    /// <returns>A list of matching company IDs.</returns>
    public async Task<List<string>> GetAllCompanyIdsWithPrefix(string prefix)
    {
      return await _context.Companies
              .Where(c => c.CompanyId.StartsWith(prefix))
              .Select(c => c.CompanyId)
              .ToListAsync();
    }
    /// <summary>
    /// Retrieves a company by its registration number.
    /// </summary>
    /// <param name="regNumber">The company registration number.</param>
    /// <returns>The matching Company entity, or null if not found.</returns>
    public async Task<Company?> GetCompanyByRegNumberAsync(string regNumber)
    {
      return await _context.Companies
              .FirstOrDefaultAsync(c => c.RegistrationNumber == regNumber);
    }
    /// <summary>
    /// Retrieves a company by its UIF number.
    /// </summary>
    /// <param name="uifNumber">The UIF number of the company.</param>
    /// <returns>The matching Company entity, or null if not found.</returns>
    public async Task<Company?> GetCompanyByUIFAsync(string uifNumber)
    {
      return await _context.Companies
              .FirstOrDefaultAsync(c => c.UIFNumber == uifNumber);
    }
    /// <summary>
    /// Retrieves a company by its VAT number.
    /// </summary>
    /// <param name="vatNumber">The VAT number of the company.</param>
    /// <returns>The matching Company entity, or null if not found.</returns>
    public async Task<Company?> GetCompanyByVATAsync(string vatNumber)
    {
      return await _context.Companies
              .FirstOrDefaultAsync(c => c.VATNumber == vatNumber);
    }
    /// <summary>
    /// Retrieves a company by its contact number.
    /// </summary>
    /// <param name="contactNumber">The contact number of the company.</param>
    /// <returns>The matching Company entity, or null if not found.</returns>
    public async Task<Company?> GetCompanyByContactNumberAsync(string contactNumber)
    {
      return await _context.Companies
              .FirstOrDefaultAsync(c => c.ContactNumber == contactNumber);
    }

    /// <summary>
    /// Retrieves a company by its name.
    /// </summary>
    /// <param name="companyName">The name of the company.</param>
    /// <returns>The matching Company entity, or null if not found.</returns>
    public async Task<Company?> GetCompanyByNameAsync(string companyName)
    {
      return await _context.Companies
              .FirstOrDefaultAsync(c => c.CompanyName == companyName);
    }

  }
}