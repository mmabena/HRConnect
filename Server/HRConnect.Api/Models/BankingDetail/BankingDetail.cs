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
    AlbarakaBank,
    BankZero,
    BidvestBank,
    Capitec,
    Citibank,
    DiscoveryBank,
    FNB,
    FirstRandBank,
    GBSMutualBank,
    GrindrodBank,
    HBZBank,
    HSBC,
    Investec,
    JPMorganChase,
    LandBank,
    MercantileBank,
    Nedbank,
    Postbank,
    SasfinBank,
    StandardBank,
    StandardChartered,
    StateBankOfIndia,
    TymeBank,
    Ubank,
    YWBNMutualBank
}

    public enum AccountType
    {
        Savings,
        Cheque,
        Current,
        Business
    }

    public enum PayFrequency
    {
        Weekly,
        BiWeekly,
        Monthly
    }

    public enum PaymentMethod
    {
        EFT,
        Cheque,
        Cash

    }

    public enum ReferenceTypes
    {
        salary,
        bonus,
        other
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
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public PayFrequency PayFrequency { get; set; }
        
        [Required]
        public ReferenceTypes ReferenceType { get; set; }

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