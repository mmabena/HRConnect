namespace HRConnect.Api.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.DTOs.BankingDetails;
    using HRConnect.Api.Models;

    public static class BankingDetailsMapper
    {
        public static BankingDetails ToBankingDetails(this CreateBankingDetailsDto createBankingDetailsDto)
        {
            return new BankingDetails
            {
                BankName = createBankingDetailsDto.BankName,
                AccountNumber = createBankingDetailsDto.AccountNumber,
                BranchCode = createBankingDetailsDto.BranchCode,
                IsActive = true, // Set default value for IsActive
                CreatedAt = DateTime.UtcNow
            };
        }

        public static BankingDetailsDto ToBankingDetailsDto(this BankingDetails bankingDetails)
        {
            return new BankingDetailsDto
            {
                BankingDetailsId = bankingDetails.BankingDetailsId,
                BankName = bankingDetails.BankName,
                AccountNumber = bankingDetails.AccountNumber,
                BranchCode = bankingDetails.BranchCode,
                CreatedAt = bankingDetails.CreatedAt,
                UpdatedAt = bankingDetails.UpdatedAt,
                IsActive = bankingDetails.IsActive
            };
        }
    }
}