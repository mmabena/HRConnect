namespace HRConnect.Api.Mappers
{
    using System;
    using HRConnect.Api.DTOs.BankingDetails;
    using HRConnect.Api.Models;

    public static class BankingDetailsMapper
    {
        // CREATE → ENTITY
        public static BankingDetail ToBankingDetails(this CreateBankingDetailDto dto, string encryptedAccount, string hash, string last4)
        {
            return new BankingDetail
            {
                Name = dto.Name,
                Surname = dto.Surname,
                IdNumber = dto.IdNumber,
                PassportNumber = dto.PassportNumber,

                BankName = dto.BankName,
                AccountType = dto.AccountType,

                AccountNumberEncrypted = encryptedAccount,
                AccountNumberSearchHash = hash,
                AccountNumberLast4Digits = last4,
                BranchCode = dto.BranchCode,

                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }


        public static BankingDetailDto ToBankingDetailDto(this BankingDetail entity)
        {
            return new BankingDetailDto
            {
                BankingDetailsId = entity.BankingDetailsId,
                Name = entity.Name,
                Surname = entity.Surname,
                IdNumber = entity.IdNumber,
                PassportNumber = entity.PassportNumber,

                BankName = entity.BankName,
                AccountType = entity.AccountType,

                // For security reasons, we do not return the actual account number.
                //  Instead, we return a masked version or simply indicate that it exists.
             AccountNumber = "**** **** " + entity.AccountNumberLast4Digits,
                BranchCode = entity.BranchCode,

                NetSalary = entity.NetSalary,
                IsActive = entity.IsActive,

                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}