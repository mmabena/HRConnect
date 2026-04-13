namespace HRConnect.Api.DTOs.BankingDetails
{
  using System.Collections.Generic;
  using System;
  using HRConnect.Api.Models;
  public class BankingDetailDto
  {
    public int BankingDetailsId { get; set; }   

    public string EmployeeId { get; set; } = string.Empty;
 
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string IdNumber { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public BankName BankName { get; set; } 
    public string AccountNumber { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public string BranchCode { get; set; } = string.Empty;
     public decimal? NetSalary { get; set; }
    public bool IsActive { get; set; }
     public DateTime CreatedAt { get; set; }
     public DateTime UpdatedAt { get; set; }

  }
}