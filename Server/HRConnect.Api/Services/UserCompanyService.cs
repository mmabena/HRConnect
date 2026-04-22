namespace HRConnect.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HRConnect.Api.Models;
    using HRConnect.Api.Mappers;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.DTOs.UserCompany;
    using System.Threading.Tasks;
    public class UserCompanyService : IUserCompanyService
    {
        private readonly IUserCompanyRepository _userCompanyRepo;
        private readonly IUserRepository _userRepo;
        private readonly ICompanyRepository _companyRepo;

        public UserCompanyService(IUserCompanyRepository userCompanyRepo, IUserRepository userRepo, ICompanyRepository companyRepo)
        {
            _userCompanyRepo = userCompanyRepo;
            _userRepo = userRepo;
            _companyRepo = companyRepo;
        }

        public async Task<List<UserCompanyDto>> GetMyCompaniesAsync(int userId)
        {
            var companies = await _userCompanyRepo.GetUserCompaniesByUserIdAsync(userId);

            return companies
                .Select(uc => uc.ToUserCompanyDto())
                .ToList();

        }
        public async Task AssignCompanyToUserAsync(int userId, CreateUserCompanyDto userCompanyRequestDto)
        {
            await ValidateAssigning(userId, userCompanyRequestDto.CompanyId);

            var existingUserCompanies = await _userCompanyRepo.GetUserCompaniesByUserIdAsync(userId);

            bool isFirstCompany = existingUserCompanies.Count == 0;

            var createdUserCompany = userCompanyRequestDto.ToUserCompanyFromCreateDTO(userId);

            createdUserCompany.IsDefault = isFirstCompany;

            await _userCompanyRepo.CreateUserCompanyAsync(createdUserCompany);
        }
        public async Task SwitchCompanyAsync(int userId, string companyId)
        {
            var isLinked = await _userCompanyRepo.UserCompanyExistsAsync(userId, companyId);
            if (!isLinked)
                throw new UnauthorizedAccessException($"User {userId} is not linkeed to this company({companyId})");

            var userCompanies = await _userCompanyRepo.GetUserCompaniesByUserIdAsync(userId);

            foreach (var uc in userCompanies)
            {
                uc.IsDefault = false;
            }

            var selectedCompany = userCompanies.First(uc => uc.CompanyId == companyId);
            selectedCompany.IsDefault = true;

            await _userCompanyRepo.UpdateRangeAsync(userCompanies);

        }
        private async Task ValidateAssigning(int userId, string companyId)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null)
                throw new ArgumentException($"User with Id {userId} does not exist.");

            var company = await _companyRepo.GetCompanyByIdAsync(companyId);
            if (company == null)
                throw new ArgumentException($"Company with ID {companyId} does not exist.");

            var existingCompanies = await _userCompanyRepo.GetUserCompaniesByUserIdAsync(userId);

            if (existingCompanies.Any(c => c.CompanyId == companyId))
                throw new InvalidOperationException("User is already assigned to this company.");

            if (user.Role == UserRole.NormalUser && existingCompanies.Count >= 1)
                throw new UnauthorizedAccessException("Normal users can only be assigned to one company.");

        }

    }
}