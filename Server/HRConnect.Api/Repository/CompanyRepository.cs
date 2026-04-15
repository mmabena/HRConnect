namespace HRConnect.Api.Repository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models;
    using HRConnect.Api.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDBContext _context;
        public CompanyRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<Company>> GetAllCompaniesAsync()
        {
            return await _context.Companies
                    .ToListAsync();
            
        }
        public async Task<Company?> GetCompanyByIdAsync(string companyId)
        {
            return await _context.Companies
                    .FirstOrDefaultAsync(c => c.CompanyId == companyId);
        }
        public async Task<Company> CreateCompanyAsync(Company companyModel)
        {
            await _context.Companies.AddAsync(companyModel);
            await _context.SaveChangesAsync();
            return companyModel;
            
        }
        public async Task<List<string>> GetAllCompanyIdsWithPrefix(string prefix)
        {
            return await _context.Companies
                    .Where(c => c.CompanyId.StartsWith(prefix))
                    .Select(c => c.CompanyId)
                    .ToListAsync();
        }
        public async Task<Company?> GetCompanyByRegNumberAsync(string regNumber)
        {
            return await _context.Companies
                    .FirstOrDefaultAsync(c => c.RegistrationNumber == regNumber);
        }
        public async Task<Company?> GetCompanyByUIFAsync(string uifNumber)
        {
            return await _context.Companies
                    .FirstOrDefaultAsync(c => c.UIFNumber == uifNumber); 
        }
        public async Task<Company?> GetCompanyByVATAsync(string vatNumber)
        {
            return await _context.Companies
                    .FirstOrDefaultAsync(c => c.VATNumber == vatNumber);
        }
        public async Task<Company?> GetCompanyByContactNumberAsync(string contactNumber)
        {
            return await _context.Companies
                    .FirstOrDefaultAsync(c => c.ContactNumber == contactNumber);
        }

    }
}