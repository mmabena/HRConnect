namespace HRConnect.Api.Interfaces
{
  using System.Collections.Generic;
  using HRConnect.Api.Models;
  using System.Threading.Tasks;

  public interface ICompanyRepository
  {
    Task<List<Company>> GetAllCompaniesAsync();
    Task<Company?> GetCompanyByIdAsync(string companyId);
    Task<Company> CreateCompanyAsync(Company companyModel);
    Task<List<string>> GetAllCompanyIdsWithPrefix(string prefix);
    Task<Company?> GetCompanyByRegNumberAsync(string regNumber);
    Task<Company?> GetCompanyByUIFAsync(string uifNumber);
    Task<Company?> GetCompanyByVATAsync(string vatNumber);
    Task<Company?> GetCompanyByContactNumberAsync(string contactNumber);
    Task<Company?> GetCompanyByNameAsync(string companyName);
  }
}