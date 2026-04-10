namespace HRConnect.Api.DTOs.BankingDetails
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using HRConnect.Api.Models;
    public class CreateBankingDetailsDto
    {
        public int TemporaryEmployeeId { get; set; }
        public string Name { get; set; } = string.Empty;

        public string Surname { get; set; } = string.Empty;
        public string? IdNumber { get; set; } = string.Empty;
        public string? PassportNumber { get; set; } = string.Empty;

        public string BankName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;

        

    }
}