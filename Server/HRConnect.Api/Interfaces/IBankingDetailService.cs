namespace HRConnect.Api.Services
{
      using HRConnect.Api.DTOs.BankingDetails;
    using System.Collections.Generic;
    using System.Threading.Tasks;   

    public interface IBankingDetailService
    {

        Task<BankingDetailDto> GetBankingDetailsAsync(string employeeId);
        Task<BankingDetailDto> CreateBankingDetailsAsync(CreateBankingDetailDto createBankingDetailsDto);
        Task<BankingDetailDto> UpdateBankingDetailsAsync(string employeeId, UpdateBankingDetailDto updatebankingDetailsDto);
    }
}   