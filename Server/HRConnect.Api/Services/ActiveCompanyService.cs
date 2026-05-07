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
            var companies = await _context.UserCompanies
                .Where(uc => uc.UserId == userId)
                .ToListAsync();

            if (companies.Count == 0)
                throw new UnauthorizedAccessException("User not linked to any company.");

            var active = companies.FirstOrDefault(uc => uc.IsDefault);

            if (active != null)
                return active.CompanyId;

            if (companies.Count == 1)
                return companies.First().CompanyId;

            throw new InvalidOperationException("No active company set.");
        }
    }
}