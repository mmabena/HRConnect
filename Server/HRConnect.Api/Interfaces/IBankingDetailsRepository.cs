namespace HRConnect.Api.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    using HRConnect.Api.DTOs.BankingDetails;

    public interface IBankingDetailsRepository
    {
        Task<BankingDetails> CreateBankingDetailsAsync(BankingDetails bankingDetails);
        Task<BankingDetails?> GetByTempEmployeeIdAsync(int tempEmployeeId);
        Task<BankingDetails> UpdateBankingDetailsAsync(BankingDetails bankingDetails);
  
    }
}