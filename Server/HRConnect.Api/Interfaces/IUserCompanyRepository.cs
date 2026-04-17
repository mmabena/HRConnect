namespace HRConnect.Api.Interfaces
{
    using System;
    using HRConnect.Api.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    public interface IUserCompanyRepository
    {
        Task<bool> UserCompanyExistsAsync(int userId, string companyId);
        Task<UserCompany> CreateUserCompanyAsync(UserCompany userCompanyModel);
        Task<List<UserCompany>> GetUserCompaniesByUserIdAsync(int userId);
    }
}