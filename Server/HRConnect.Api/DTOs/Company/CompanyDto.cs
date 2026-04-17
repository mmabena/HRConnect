namespace HRConnect.Api.DTOs.Company
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.Models;
  public class CompanyDto
  {
    public string CompanyId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string UIFNumber { get; set; } = string.Empty;
    public string? VATNumber { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
  }
}