namespace HRConnect.Api.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.SignalR;
    using HRConnect.Api.Hubs;
    using HRConnect.Api.Models;
    using HRConnect.Api.Mappers;
    using Microsoft.EntityFrameworkCore;
    using HRConnect.Api.Data;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.DTOs.UserCompany;
    using System.Threading.Tasks;
    public class UserCompanyService : IUserCompanyService
    {
        private readonly IUserCompanyRepository _userCompanyRepo;
        private readonly IUserRepository _userRepo;
        private readonly ICompanyRepository _companyRepo;
        private readonly ApplicationDBContext _context;
        private readonly IHubContext<CompanyHub> _companyHubContext;
        public UserCompanyService(ApplicationDBContext context, IUserCompanyRepository userCompanyRepo, IUserRepository userRepo, ICompanyRepository companyRepo, IHubContext<CompanyHub> companyHubContext)
        {
            _userCompanyRepo = userCompanyRepo;
            _userRepo = userRepo;
            _companyRepo = companyRepo;
            _context = context;
            _companyHubContext = companyHubContext;
        }
        /// <summary>
        /// Retrives the list of companies assigned to the user. 
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The list of companies assigned to the user.</returns>
        public async Task<List<UserCompanyDto>> GetMyCompaniesAsync(int userId)
        {
            var companies = await _userCompanyRepo.GetUserCompaniesByUserIdAsync(userId);

            var user = await _userRepo.GetUserByIdAsync(userId);

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == user.Email);

            var originalCompanyId = employee?.CompanyId;

            var result = new List<UserCompanyDto>();

            foreach (var company in companies)
            {
                var employeeCount = await _context.Employees
                  .CountAsync(e => e.CompanyId == company.CompanyId);

                result.Add(new UserCompanyDto
                {
                    CompanyId = company.CompanyId,
                    CompanyName = company.Company.CompanyName,
                    RegistrationNumber = company.Company.RegistrationNumber,
                    IsDefault = company.IsDefault,
                    IsOriginalCompany = company.CompanyId == originalCompanyId,
                    EmployeeCount = employeeCount
                });
            }
            return result;

        }
        /// <summary>
        /// Create a new company assignment to a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="userCompanyRequestDto">The company assignement model to assign a user to a company.</param>
        /// <returns>The created UserCompany relationship.</returns>
        public async Task AssignCompanyToUserAsync(int userId, CreateUserCompanyDto userCompanyRequestDto)
        {
            await ValidateAssigning(userId, userCompanyRequestDto.CompanyId);

            var existingUserCompanies = await _userCompanyRepo.GetUserCompaniesByUserIdAsync(userId);

            bool isFirstCompany = existingUserCompanies.Count == 0;

            var createdUserCompany = userCompanyRequestDto.ToUserCompanyFromCreateDTO(userId);

            createdUserCompany.IsDefault = isFirstCompany;

            await _userCompanyRepo.CreateUserCompanyAsync(createdUserCompany);
        }
        /// <summary>
        /// Switches the active company for a user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="companyId">The company ID.</param>
        /// <returns>A verification message along with the updated UserCompany information.</returns>
        public async Task SwitchCompanyAsync(int userId, string companyId)
        {
            var isLinked = await _userCompanyRepo.UserCompanyExistsAsync(userId, companyId);
            if (!isLinked)
                throw new UnauthorizedAccessException($"User {userId} is not linkeed to this company({companyId})");

            var userCompanies = await _userCompanyRepo.GetUserCompaniesByUserIdAsync(userId);

            if (userCompanies.Count == 1)
            {
                return;
            }
                // throw new InvalidOperationException("Cannot switch company: User is only linked to one company.");

            foreach (var uc in userCompanies)
            {
                uc.IsDefault = false;
            }

            var selectedCompany = userCompanies.First(uc => uc.CompanyId == companyId);
            selectedCompany.IsDefault = true;

            await _userCompanyRepo.UpdateRangeAsync(userCompanies);

            await _companyHubContext.Clients.All.SendAsync(
                "CompanySwitched",
                new
                {
                    UserId = userId,
                    CompanyId = companyId
                }
            );
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