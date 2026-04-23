namespace HRConnect.Api.DTOs.BankingDetails
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using HRConnect.Api.Models;
    public class CreateBankingDetailDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string IdNumber { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public BankName BankName { get; set; } 
        public int BankBranchCodeId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public AccountType AccountType { get; set; }
        public bool IsActive { get; set; } = true;


    }
}