namespace HRConnect.Api.Repository
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Data;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models.Payroll.Earning;
  using Microsoft.EntityFrameworkCore;

  public class PayrollEarningRepository(ApplicationDBContext context) : IPayrollEarningRepository
  {
    private readonly ApplicationDBContext _context = context;

    ///<summary>
    ///Add new payroll earning to the database
    ///</summary>
    ///<param name="payrollEarning"> Pay roll earning model</param>
    ///<returns>
    ///PayrollEarning object with the details of the added payroll earning  
    /// </returns>
    public async Task<PayrollEarning> AddAsync(PayrollEarning payrollEarning)
    {
      _ = await _context.PayrollEarnings.AddAsync(payrollEarning);
      _ = await _context.SaveChangesAsync();
      return payrollEarning;
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
      return await _context.PayrollEarnings.AnyAsync(pre =>
        EF.Functions.Like(pre.ShortDescription, shortDescription) && EF.Functions.Like(pre.LongDescription, longDescription));
    }


    ///<summary>
    ///Delete a payroll earning by setting its status to inactive
    ///</summary>
    ///<param name="payrollEarningId">Pay roll earning Id</param>
    ///<returns>
    ///A message indicating that the payroll earning has been set to inactive
    /// </returns>
    public async Task<string> DeleteAsync(string payrollEarningId)
    {
      PayrollEarning payrollEarning = await _context.PayrollEarnings.Where(pre => pre.PayrollEarningId == payrollEarningId).FirstOrDefaultAsync()
        ?? throw new KeyNotFoundException("Payroll Earning not found");

      payrollEarning.IsActive = false;
      _ = _context.PayrollEarnings.Update(payrollEarning);
      _ = _context.SaveChangesAsync();
      return $"Pay roll earning with Id: {payrollEarningId} has been set to inactive";
    }

    ///<summary>
    ///Retrieve all payroll earnings from the database
    ///</summary>
    ///<returns>
    ///A list of PayrollEarning objects representing all payroll earnings in the database
    ///</returns>
    public async Task<List<PayrollEarning>> GetAllAsync()
    {
      return await _context.PayrollEarnings.ToListAsync();
    }

    ///<summary>
    ///Retrieve all payroll earning Ids that start with the given prefix from the database   
    ///</summary>
    ///<param name="prefix">String prefix</param>
    ///<returns>
    ///A list of strings representing all payroll earning Ids that start with the given prefix in the database
    /// </returns>
    public async Task<List<string>> GetAllPayrollEarningIdsAsync(string prefix)
    {
      return await _context.PayrollEarnings
          .Where(pre => pre.PayrollEarningId.StartsWith(prefix))
          .Select(pre => pre.PayrollEarningId)
          .ToListAsync();
    }

    ///<summary>
    ///Retrieve payroll earning details by payroll earning Id from the database
    ///</summary>
    ///<param name="payrollEarningId">Pay roll earning Id</param>
    ///<returns>
    ///Pay roll earning object with the details of the given payroll earning Id, if found in the database, otherwise null
    ///</returns>
    public async Task<PayrollEarning?> GetByPayrollEarningId(string payrollEarningId)
    {
      return await _context.PayrollEarnings.Where(pre => pre.PayrollEarningId == payrollEarningId).FirstOrDefaultAsync();
    }

    ///<summary>
    ///Retrieve payroll earning details by tax code from the database
    ///</summary>
    ///<param name="taxCode">Tax code</param>
    ///<returns>
    ///A list of payroll earning objects with the details of the given tax code, if found in the database, otherwise an empty list
    /// </returns>
    public async Task<List<PayrollEarning>> GetByTaxCode(int taxCode)
    {
      return await _context.PayrollEarnings.Where(pre => pre.TaxCode == taxCode).ToListAsync();
    }

    ///<summary>
    ///Update payroll earning details in the database
    ///</summary>
    ///<param name="payrollEarning">Pay roll earning model</param>
    ///<returns>
    ///The updated payroll earning object
    ///</returns>
    public async Task<PayrollEarning> UpdateAsync(PayrollEarning payrollEarning)
    {
      _ = _context.PayrollEarnings.Update(payrollEarning);
      _ = await _context.SaveChangesAsync();
      return payrollEarning;
    }
  }
}
