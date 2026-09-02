namespace HRConnect.Api.DTOs.BankingDetails
{
    using System.Collections.Generic;
    using System;
    using HRConnect.Api.Models;

    public class BankBranchCodeDto
    {
        public int BankBranchCodeId {get; set;}
         public string BankName { get; set; } = string.Empty;
        public string UniversalCode { get; set; } = string.Empty;

        public DateTime CreatedAt {get; set;} = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
       
    }
}