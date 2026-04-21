namespace HRConnect.Api.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    using HRConnect.Api.DTOs.BankingDetails;
    using System.Linq.Expressions;

    public interface IBankingDetailRepository
    {
        Task<List<BankingDetail>> GetAllBankingDetailsAsync();
        Task<BankingDetail?> GetBankingDetailsByEmployeeIdAsync(string EmployeeId);
        Task<BankingDetail> CreateBankingDetailsAsync(BankingDetail bankingDetails);
        Task<BankingDetail> UpdateBankingDetailsAsync(BankingDetail bankingDetails);
        Task LockBankingDetailsAsync();
        Task<bool> AnyAsync(Expression<Func<BankingDetail, bool>> predicate);

  
    }
}