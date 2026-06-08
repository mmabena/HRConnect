namespace HRConnect.Api.Interfaces
{
  using System.Collections.Generic;
  using HRConnect.Api.DTOs.UserCompany;
  using System.Threading.Tasks;
  public interface IUserCompanyService
  {
    Task<List<UserCompanyDto>> GetMyCompaniesAsync(int userId);
    Task AssignCompanyToUserAsync(int userId, CreateUserCompanyDto userCompanyRequestDto);
    Task SwitchCompanyAsync(int userId, string companyId);
  }
}