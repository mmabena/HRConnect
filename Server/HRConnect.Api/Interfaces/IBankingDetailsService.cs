namespace HRConnect.Api.Services
{
      using HRConnect.Api.DTOs.BankingDetails;
    using System.Collections.Generic;
    using System.Threading.Tasks;   

    public interface IBankingDetailsService
    {
        Task<BankingDetailsDto> CreateBankingDetailsAsync(CreateBankingDetailsDto createBankingDetailsDto);

        Task<BankingDetailsDto> GetByTempEmployeeIdAsync(int tempEmployeeId);

        Task<BankingDetailsDto> UpdateBankingDetailsAsync(int tempEmployeeId, UpdateBankingDetailsDto updateBankingDetailsDto);
    }
}   