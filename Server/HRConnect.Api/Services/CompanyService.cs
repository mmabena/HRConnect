namespace HRConnect.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Models;
    using System.Globalization;
    using HRConnect.Api.DTOs.Company;
    using System.IO;
    using HRConnect.Api.Data;
    using HRConnect.Api.Mappers;
    using Microsoft.EntityFrameworkCore;
    using HRConnect.Api.Interfaces;
    public class CompanyService : ICompanyService
    {
        private readonly ApplicationDBContext _context;
        private readonly ICompanyRepository _companyRepo;

        public CompanyService(ApplicationDBContext context, ICompanyRepository companyRepo)
        {
            _context = context;
            _companyRepo = companyRepo;
        }

        public async Task<List<CompanyDto>> GetAllCompaniesAsync()
        {
            var companies = await _companyRepo.GetAllCompaniesAsync();
            return companies.Select(c => c.ToCompanyDto()).ToList();
        }

        public async Task<CompanyDto?> GetCompanyByIdAsync(string companyId)
        {
            var company = await _companyRepo.GetCompanyByIdAsync(companyId);
            return company?.ToCompanyDto();
        }
        public async Task<CompanyDto> CreateCompanyAsync(CreateCompanyRequestDto companyRequestDto)
        {
            await ValidateCreate(companyRequestDto);
            companyRequestDto.CompanyId = await GenerateCompanyId(companyRequestDto.CompanyName);

            var new_company = companyRequestDto.ToCompanyFromCreateDTO();

            var createdCompany = await _companyRepo.CreateCompanyAsync(new_company);
            return createdCompany.ToCompanyDto();

        }

        private async Task<string> GenerateCompanyId(string companyName)
        {
            string prefix = companyName.Length >= 3
                ? companyName.Substring(0, 3).ToUpper(CultureInfo.InvariantCulture)
                : companyName.ToUpper(CultureInfo.InvariantCulture).PadRight(3, 'X');
            int nextNum = 1;

            var existingIds = await _companyRepo.GetAllCompanyIdsWithPrefix(prefix);
            if (existingIds.Count > 0)
            {
                var maxNum = existingIds
                        .Select(id => int.Parse(id.AsSpan(3), CultureInfo.InvariantCulture))
                        .Max();

                nextNum = maxNum + 1;
            }
            return $"{prefix}{nextNum:D3}";
        }

        private async Task ValidateCreate(CreateCompanyRequestDto companyRequestDto)
        {
            if (string.IsNullOrWhiteSpace(companyRequestDto.CompanyName))
                throw new ArgumentException("Company name is required");

            if (companyRequestDto.RegistrationNumber.Length != 14)
                throw new ArgumentException("Registration number must be 14 digits");

            if (companyRequestDto.UIFNumber.Length != 10)
                throw new ArgumentException("UIF number must be 10 digits");

            if (companyRequestDto.ContactNumber.Length != 10)
                throw new ArgumentException("Contact number must be 10 digits");

            if (!string.IsNullOrWhiteSpace(companyRequestDto.RegistrationNumber) &&
                await _companyRepo.GetCompanyByRegNumberAsync(companyRequestDto.RegistrationNumber) != null)
                throw new InvalidOperationException("A company with the same registration number already exists");

            if (!string.IsNullOrWhiteSpace(companyRequestDto.UIFNumber) &&
                await _companyRepo.GetCompanyByUIFAsync(companyRequestDto.UIFNumber) != null)
                throw new InvalidOperationException("A company with the same UIF number already exists");

            if (!string.IsNullOrWhiteSpace(companyRequestDto.VATNumber) &&
                await _companyRepo.GetCompanyByVATAsync(companyRequestDto.VATNumber) != null)
                throw new InvalidOperationException("A company with the same VAT number already exists");

            if (!string.IsNullOrWhiteSpace(companyRequestDto.ContactNumber) &&
                await _companyRepo.GetCompanyByContactNumberAsync(companyRequestDto.ContactNumber) != null)
                throw new InvalidOperationException("A company with the same contact number already exists");

        }

    }
}