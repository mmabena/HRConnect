namespace HRConnect.Api.Interfaces
{
  using HRConnect.Api.Models;
  using System.Collections.Generic;
  using System.Threading.Tasks;
  public interface IUserCompanyRepository
  {
    Task<bool> UserCompanyExistsAsync(int userId, string companyId);
    Task<UserCompany> CreateUserCompanyAsync(UserCompany userCompanyModel);
    Task<List<UserCompany>> GetUserCompaniesByUserIdAsync(int userId);
    Task UpdateRangeAsync(List<UserCompany> userCompanies);
  }
}