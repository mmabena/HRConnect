namespace HRConnect.Api.Repository
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.PayrollDeduction;
  using HRConnect.Api.Services;
  using Microsoft.EntityFrameworkCore;

  public class DeductionRepository(ApplicationDBContext context) : IDeductionRepository
  {
    private readonly ApplicationDBContext _context = context;

    ///<summary>
    ///Add a new deduction to the database.
    ///</summary>
    ///<param name="deduction">Deduction model</param>
    ///<returns>
    ///Added deduction model 
    ///</returns>
    public async Task<Deduction> AddAsync(Deduction deduction)
    {
      _ = await _context.Deductions.AddAsync(deduction);
      _ = await _context.SaveChangesAsync();
      return deduction;
    }

    ///<summary>
    ///Check if a payroll earning with the given long description and short description already exists in the database
    ///</summary>
    ///<param name="longDescription">Long description</param>
    ///<returns>
    ///True if a payroll earning with the given long description already exists in the database, otherwise false
    /// </returns>
    public async Task<bool> CheckIfDescriptionsExists(string shortDescription, string longDescription)
    {
      return await _context.Deductions.AnyAsync(pre =>
        EF.Functions.Like(pre.ShortDescription, shortDescription) && EF.Functions.Like(pre.LongDescription, longDescription));
    }

    ///<summary>
    ///Set the status of a deduction to inactive instead of deleting it from the database.
    ///</summary>
    ///<param name="companyId">Company Id</param>
    ///<returns>
    ///Message indicating the deduction has been set to inactive.
    ///</returns>
    ///<exception cref="NotFoundException"></exception>
    public async Task<string> DeleteAsync(string code)
    {
      Deduction? deduction = await _context.Deductions.Where(d => d.DeductionId == code).FirstOrDefaultAsync()
        ?? throw new NotFoundException("Deduction not found");

      deduction.Status = false;
      _ = _context.Deductions.Update(deduction);
      _ = _context.SaveChangesAsync();
      return $"Deduction with code {code} has been set to inactive.";
    }

    ///<summary>
    ///Retrieve all deduction codes from the database that start with the specified prefix.
    ///</summary>
    ///<param name="prefix">Prefix of deduction codes</param>
    ///<returns>
    ///List of deduction codes
    ///</returns>
    public async Task<List<string>> GetAllDeductionCodesAsync(string prefix)
    {
      return await _context.Deductions.Where(d => d.DeductionId.StartsWith(prefix)).Select(d => d.DeductionId).ToListAsync();
    }

    ///<summary>
    ///Retrieve all deductions from the database. 
    ///</summary>
    ///<returns>
    ///A list of deduction models.
    ///</returns>
    public async Task<List<Deduction>> GetAllDeductionsAsync()
    {
      return await _context.Deductions.ToListAsync();
    }

    ///<summary>
    ///Retrieve a deduction from the database using the deduction code. 
    ///</summary>
    ///<param name="code">Deduction code</param>
    ///<returns>
    ///Deduction model with the specified code
    ///</returns>
    public async Task<Deduction?> GetDeductionByCodeAsync(string code)
    {
      return await _context.Deductions.Where(d => d.DeductionId == code).FirstOrDefaultAsync();
    }

    ///<summary>
    ///Retrieve a deduction from the database using the company Id. 
    ///</summary>
    ///<param name="companyId">Company Id</param>
    ///<returns>
    ///Deduction model with the specified company Id
    ///</returns>
    public async Task<List<Deduction>> GetDeductionByCompanyIdAsync(string companyId)
    {
      return await _context.Deductions.Where(d => d.CompanyId == companyId).ToListAsync();
    }

    ///<summary>
    ///Update an existing deduction in the database. 
    ///</summary>
    ///<param name="deduction">Deduction model</param>
    ///<returns>
    ///Updated deduction model
    ///</returns>
    public async Task<Deduction> UpdateAsync(Deduction deduction)
    {
      _ = _context.Deductions.Update(deduction);
      _ = await _context.SaveChangesAsync();
      return deduction;
    }
  }
}
