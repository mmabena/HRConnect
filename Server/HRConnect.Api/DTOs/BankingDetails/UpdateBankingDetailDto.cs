namespace HRConnect.Api.DTOs.BankingDetails
{
    using HRConnect.Api.Models;
    using System.ComponentModel.DataAnnotations;
    public class UpdateBankingDetailDto
    {

        [Required]
        [EnumDataType(typeof(BankName))]
        public BankName BankName { get; set; } 

        [Required]
        [StringLength(20, MinimumLength = 6)]
        public string AccountNumber { get; set; } = string.Empty;
        [Required]
         [EnumDataType(typeof(AccountType))]
        public AccountType AccountType { get; set; } 

        [Required]
        [Range(1, int.MaxValue)]
        public int BankBranchCodeId { get; set; }
   
    }
}