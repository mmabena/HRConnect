namespace HRConnect.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Globalization;
    using HRConnect.Api.DTOs.UserCompany;
    using HRConnect.Api.Utils;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;
    using HRConnect.Api.Interfaces;
    [Route("api/userCompany")]
    [ApiController]
    public class UserCompanyController : ControllerBase
    {
        private readonly IUserCompanyService _userCompanyService;

        public UserCompanyController(IUserCompanyService userCompanyService)
        {
            _userCompanyService = userCompanyService;
        }

        [HttpGet("my-companies")]
        public async Task<IActionResult> GetMyCompanies()
        {
            var userId = User.GetUserId();

            var companies = await _userCompanyService.GetMyCompaniesAsync(userId);
            return Ok(companies);
        }

        [HttpPost("assign/{userId}")]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> AssignCompany(int userId, [FromBody] CreateUserCompanyDto userCompanyDto)
        {
            await _userCompanyService.AssignCompanyToUserAsync(userId, userCompanyDto);
            return Ok("Company assigned successfully");
        }

        [HttpPost("switch-company")]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> SwitchCompany(string companyId)
        {
            var userId = User.GetUserId(); 

            await _userCompanyService.SwitchCompanyAsync(userId, companyId);
            return Ok("Company switched successfully");
        }
    }
}