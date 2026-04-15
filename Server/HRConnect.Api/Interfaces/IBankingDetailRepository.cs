namespace HRConnect.Api.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    using HRConnect.Api.DTOs.BankingDetails;

    public interface IBankingDetailRepository
    {
        Task<BankingDetail> GetBankingDetailsByEmployeeIdAsync(string EmployeeId);
        Task<BankingDetail> CreateBankingDetailsAsync(BankingDetail bankingDetails);
        Task<BankingDetail> UpdateBankingDetailsAsync(BankingDetail bankingDetails);
  
    }
}