namespace HRConnect.Api.DTOs.BankingDetails
{
  using System.Collections.Generic;
  using System;
  public class BankingDetailsDto
  {
    public int BankingDetailsId { get; set; }   
    public Guid TempEmployeeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? IdNumber { get; set; } = string.Empty;
    public string? PassportNumber { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
     public decimal? NetSalry { get; set; }
    public bool IsActive { get; set; }
     public DateTime CreatedDate { get; set; }
     public DateTime UpdatedDate { get; set; }

  }
}