namespace HRConnect.Api.Interfaces
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using HRConnect.Api.DTOs.Company;
  using System.Threading.Tasks;
  public interface ICompanyService
  {
    Task<List<CompanyDto>> GetAllCompaniesAsync();
    Task<CompanyDto?> GetCompanyByIdAsync(string companyId);
    Task<CompanyDto> CreateCompanyAsync(CreateCompanyRequestDto companyRequestDto);
  }
}