namespace HRConnect.Api.Models
{
    using System;
    using HRConnect.Api.Models;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public enum BankName
    {
        Absa,
        AfricanBank,
        BidvestBank,
        Capitec,
        DiscoveryBank,
        FNB,
        GrindrodBank,
        Investec,
        Nedbank,
        StandardBank,
        TymeBank
        
    }

    public enum AccountType
    {
        Savings,
        Cheque,
        Current,
        Business
    }
    public class BankingDetail
    {
        [Key]
        public int BankingDetailsId { get; set; }

        [Required]
        public string EmployeeId { get; set; }

        public Employee Employee { get; set; } = null!;

        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Surname { get; set; } = string.Empty;

        [StringLength(13)]
        public string? IdNumber { get; set; } = string.Empty;
        public string? PassportNumber { get; set; } = string.Empty;

        [Required]
        public BankName BankName { get; set; }
        [Required]
        public string AccountNumberEncrypted { get; set; } = string.Empty;

        public string AccountNumberSearchHash { get; set; } = string.Empty;

        public string AccountNumberLast4Digits { get; set; } = string.Empty;

        [Required]
        public AccountType AccountType { get; set; }

        [Required]
        public int BankBranchCodeId { get; set; } 

        public BankBranchCode BankBranchCode { get; set; } = null!;

        public bool IsLocked { get; set; } 

        public DateTime? LockedAt { get; set; }

        public decimal? NetSalary { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}