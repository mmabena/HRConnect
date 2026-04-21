namespace HRConnect.Api.Repository
{
    using System.Linq.Expressions;
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;
    using System.Linq.Expressions;

    public class BankingDetailRepository : IBankingDetailRepository
    {
        private readonly ApplicationDBContext _context;


        public BankingDetailRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all banking details from the database.
        /// </summary>
        /// <returns> A list of all banking details.</returns>
        public async Task<List<BankingDetail>> GetAllBankingDetailsAsync()
        {
            return await _context.BankingDetails.ToListAsync();
        }

        /// <summary>
        /// Retrieves the banking details of a temporary employee by their employee ID.
        ///  This method queries the database for a BankingDetail record that matches the provided employee ID.
        ///  If a matching record is found, it is returned; otherwise, null is returned. 
        /// This allows the service layer to determine if banking details exist for a given employee and to handle cases where they do not.
        /// </summary>
        /// <param name="employeeId">The ID of the employee for whom to retrieve banking details.</param>
        /// <returns>The banking detail record if found; otherwise, null.</returns>
        public async Task<BankingDetail?> GetBankingDetailsByEmployeeIdAsync(string EmployeeId)
        {
            return await _context.BankingDetails
                .FirstOrDefaultAsync(b => b.EmployeeId == EmployeeId);
        }

        /// <summary>
        /// Creates a new BankingDetails record in the database.
        /// </summary>
        /// <param name="bankingDetails"> The banking details model to create a new employee record</param>
        /// <returns>Creates a new banking details record</returns>
        public async Task<BankingDetail> CreateBankingDetailsAsync(BankingDetail bankingDetails)
        {
            _context.BankingDetails.Add(bankingDetails);
            await _context.SaveChangesAsync();
            return bankingDetails;
        }

        /// <summary>
        /// Updates an existing BankingDetails record in the database.
        /// </summary>
        /// <param name="bankingDetails"> The banking details model to update</param>
        /// <returns>Updates the existing banking details record</returns>
        public async Task<BankingDetail> UpdateBankingDetailsAsync(BankingDetail bankingDetails)
        {
            _context.BankingDetails.Update(bankingDetails);
            await _context.SaveChangesAsync();
            return bankingDetails;
        }

        /// <summary>
        /// Locks all banking details records in the database by setting the IsLocked property to true and updating the LockedAt timestamp.
        /// This method uses the ExecuteUpdateAsync method to perform a bulk update on all records where
        /// </summary>
        /// <returns>The task representing the asynchronous operation.</returns>
        public async Task LockBankingDetailsAsync()
        {
            await _context.BankingDetails
            .Where (b => !b.IsLocked)
            .ExecuteUpdateAsync(b => b.SetProperty(bd => bd.IsLocked, true)
            .SetProperty(bd => bd.LockedAt, DateTime.UtcNow));
        }

      public async Task<bool> AnyAsync(Expression<Func<BankingDetail, bool>> predicate)
        {
            return await _context.BankingDetails.AnyAsync(predicate);
        }

    }

}