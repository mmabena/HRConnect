namespace HRConnect.Api.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HRConnect.Api.Models;
    using HRConnect.Api.DTOs.UserCompany;
    using System.Threading.Tasks;
    public interface IUserCompanyService
    {
        Task<List<UserCompanyDto>> GetMyCompaniesAsync(int userId);
        Task AssignCompanyToUserAsync(int userId, CreateUserCompanyDto userCompanyRequestDto);
        Task SwitchCompanyAsync(int userId, string companyId);
    }
}