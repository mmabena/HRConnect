namespace HRConnect.Api.Repository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using HRConnect.Api.Data;
    using HRConnect.Api.DTOs.Employee;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;

    public class BankingDetailsRepository : IBankingDetailsRepository
    {
        private readonly ApplicationDBContext _context;


        public BankingDetailsRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new BankingDetails record in the database.
        /// </summary>
        /// <param name="bankingDetails"> The banking details model to create a new employee record</param>
        /// <returns>Creates a new banking details record</returns>
        public async Task<BankingDetails> CreateBankingDetailsAsync(BankingDetails bankingDetails)
        {
            _context.BankingDetails.Add(bankingDetails);
            await _context.SaveChangesAsync();
            return bankingDetails;
        }

        /// <summary>
        /// Retrieves a single BankingDetails record by the associated TempEmployeeId.
        /// </summary>
        /// <param name="tempEmployeeId">The temporary employee ID</param>
        /// <returns>The banking details record or null if not found</returns>
        public async Task<BankingDetails?> GetByTempEmployeeIdAsync(int tempEmployeeId)
        {
            return await _context.BankingDetails.FirstOrDefaultAsync(b => b.TempEmployeeId == tempEmployeeId);
        }

        /// <summary>
        /// Updates an existing BankingDetails record in the database.
        /// </summary>
        /// <param name="bankingDetails"> The banking details model to update</param>
        /// <returns>Updates the existing banking details record</returns>
        public async Task<BankingDetails> UpdateBankingDetailsAsync(BankingDetails bankingDetails)
        {
            _context.BankingDetails.Update(bankingDetails);
            await _context.SaveChangesAsync();
            return bankingDetails;
        }


    }

}