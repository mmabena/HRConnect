namespace HRConnect.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HRConnect.Api.Mappers;
    using HRConnect.Api.Models;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.DTOs.Company;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    [Route("api/company")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> GetAllCompanies()
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            return Ok(companies);
        }

        [HttpGet("{CompanyId}")]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> GetCompanyById(string companyId)
        {
            var company = await _companyService.GetCompanyByIdAsync(companyId);
            if (company == null)
                return NotFound();

            return Ok(company);
        }

        [HttpPost]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequestDto companyRequestDto)
        {
            var company = await _companyService.CreateCompanyAsync(companyRequestDto);
            return CreatedAtAction(nameof(GetCompanyById), new { companyId = company.CompanyId}, company);
        }


    }
}