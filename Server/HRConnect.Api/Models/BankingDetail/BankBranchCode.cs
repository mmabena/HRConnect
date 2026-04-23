namespace HRConnect.Api.Models
{
    using System;
    using HRConnect.Api.Models;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class BankBranchCode
    {
        [Key]
        public int BankBranchCodeId { get; set; }
        [Required]
        public string BankName { get; set; } = string.Empty;
        [Required]
        public string UniversalCode { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BankingDetail> BankingDetails { get; set; }
    }
}