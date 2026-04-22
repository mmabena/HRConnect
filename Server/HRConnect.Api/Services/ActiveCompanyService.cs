namespace HRConnect.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Data;
    using System.Globalization;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;
    public class ActiveCompanyService : IActiveCompanyService
    {
        private readonly ApplicationDBContext _context;
        public ActiveCompanyService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<string> GetActiveCompanyIdAsync(int userId)
        {
            var companyId = await _context.UserCompanies
                .Where(uc => uc.UserId == userId && uc.IsDefault)
                .Select(uc => uc.CompanyId)
                .FirstOrDefaultAsync();
            
            if (companyId == null)
                throw new InvalidOperationException("No active company set for this user.");

            return companyId;
        }
    }
}