namespace HRConnect.Api.DTOs.BankingDetails
{
    using HRConnect.Api.Models;
    public class UpdateBankingDetailDto
    {
        public BankName BankName { get; set; } 
        public string AccountNumber { get; set; } = string.Empty;
        public AccountType AccountType { get; set; } 
        public string BranchCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}