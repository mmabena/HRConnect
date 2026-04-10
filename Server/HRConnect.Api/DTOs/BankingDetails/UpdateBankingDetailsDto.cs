namespace HRConnect.Api.DTOs.BankingDetails
{
using HRConnect.Api.Models;
    public class UpdateBankingDetailsDto
    {
        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}