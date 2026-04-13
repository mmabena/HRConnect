namespace HRConnect.Api.Mappers
{
    using System;
    using HRConnect.Api.DTOs.BankingDetails;
    using HRConnect.Api.Models;

    public static class BankingDetailsMapper
    {
        // CREATE → ENTITY
        public static BankingDetail ToBankingDetails(this CreateBankingDetailDto dto)
        {
            return new BankingDetail
            {
                Name = dto.Name,
                Surname = dto.Surname,
                IdNumber = dto.IdNumber,
                PassportNumber = dto.PassportNumber,

                BankName = dto.BankName,
                AccountType = dto.AccountType,

                AccountNumber = dto.AccountNumber,
                BranchCode = dto.BranchCode,

                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        // ENTITY → DTO
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

                AccountNumber = entity.AccountNumber,
                BranchCode = entity.BranchCode,

                NetSalary = entity.NetSalary,
                IsActive = entity.IsActive,

                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}