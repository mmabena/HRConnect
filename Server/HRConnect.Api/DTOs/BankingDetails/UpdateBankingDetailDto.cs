namespace HRConnect.Api.DTOs.BankingDetails
{
    using HRConnect.Api.Models;
    using System.ComponentModel.DataAnnotations;
    public class UpdateBankingDetailDto
    {
        public BankName BankName { get; set; } 
        public string AccountNumber { get; set; } = string.Empty;
        public AccountType AccountType { get; set; } 
        public string BranchCode { get; set; } = string.Empty;
   
    }
}