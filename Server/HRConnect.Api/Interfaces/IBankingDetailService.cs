namespace HRConnect.Api.Services
{
      using HRConnect.Api.DTOs.BankingDetails;
    using System.Collections.Generic;
    using System.Threading.Tasks;   

    public interface IBankingDetailService
    {
        Task <List<BankingDetailDto>> GetAllBankingDetailsAsync();
        Task <List<BankBranchCodeDto>> GetAllBankBranchCodesAsync();
        Task<BankingDetailDto> GetBankingDetailsByEmployeeIdAsync(string EmployeeId);
        Task<BankingDetailDto> CreateBankingDetailsAsync(CreateBankingDetailDto createBankingDetailsDto);
        Task<BankingDetailDto> UpdateBankingDetailsAsync(string EmployeeId, UpdateBankingDetailDto updatebankingDetailsDto);
        Task LockAllBankingDetailsAsync();
    }
}   