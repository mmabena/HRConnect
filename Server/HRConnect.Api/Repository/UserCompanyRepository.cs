namespace HRConnect.Api.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Storage;
    using HRConnect.Api.Data;
    using System.Threading.Tasks;
    public class UserCompanyRepository : IUserCompanyRepository
    {
        private readonly ApplicationDBContext _context;

        public UserCompanyRepository( ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<bool> UserCompanyExistsAsync(int userId, string companyId)
        {
            return await _context.UserCompanies
                .AnyAsync(uc => uc.UserId == userId && uc.CompanyId == companyId);
            
        }
        public async Task<UserCompany> CreateUserCompanyAsync(UserCompany userCompanyModel)
        {
            await _context.UserCompanies.AddAsync(userCompanyModel);
            await _context.SaveChangesAsync();
            return userCompanyModel;
            
        }
        public async Task<List<UserCompany>> GetUserCompaniesByUserIdAsync(int userId)
        {
            return await _context.UserCompanies
                .Include(uc => uc.Company)
                .Where(uc => uc.UserId == userId)
                .ToListAsync();
        }
        public async Task UpdateRangeAsync(List<UserCompany> userCompanies)
        {
            _context.UserCompanies.UpdateRange(userCompanies);
            await _context.SaveChangesAsync();
        }

    }
}