namespace HRConnect.Api.Repository
{
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;

    public class BankingDetailRepository : IBankingDetailRepository
    {
        private readonly ApplicationDBContext _context;


        public BankingDetailRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves the banking details of a temporary employee by their employee ID.
        ///  This method queries the database for a BankingDetail record that matches the provided employee ID.
        ///  If a matching record is found, it is returned; otherwise, null is returned. 
        /// This allows the service layer to determine if banking details exist for a given employee and to handle cases where they do not.
        /// </summary>
        /// <param name="employeeId">The ID of the employee for whom to retrieve banking details.</param>
        /// <returns>The banking detail record if found; otherwise, null.</returns>
        public async Task<BankingDetail> GetBankingDetailsByEmployeeIdAsync(string employeeId)
        {
            return await _context.BankingDetails
                .FirstOrDefaultAsync(b => b.EmployeeId == employeeId);
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


    }

}