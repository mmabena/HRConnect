namespace HRConnect.Api.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;


    public enum BankName
    {
        Absa,
        AfricanBank,
        BidvestBank,
        Capitec,
        DiscoverBank,
        FNB,
        GrindrodBank,
        Investec,
        Nedbank,
        StandardBank,


    }
    public class BankingDetails
    {
        [Key]
        public int BankingDetailsId { get; set; }
       
       public int TempEmployeeId { get; set; } 
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Surname { get; set; } = string.Empty;

        [StringLength(13)]
        public string? IdNumber { get; set; } = string.Empty;

        [Required]
        public string? PassportNumber { get; set; } = string.Empty;

        [Required]
        public string BankName { get; set; } = string.Empty;
        [Required]
        public string AccountNumber { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public decimal? NetSalry { get; set; }
        public bool IsActive { get; set; }
         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}