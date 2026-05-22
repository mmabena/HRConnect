namespace HRConnect.Api.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HRConnect.Api.DTOs.CompanyContribution;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Models.CompanyContributions;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    [Route("api/companyContribution")]
    [ApiController]
    public class CompanyContributionsController : ControllerBase
    {
        private readonly ICompanyContributionService _companyContributionService;

        public CompanyContributionsController(ICompanyContributionService companyContributionService)
        {
            _companyContributionService = companyContributionService;
        }

        [HttpGet]
        [Authorize(Roles = "SuperUser")]
        public async Task<ActionResult<List<CompanyContributionDto>>> GetAllCompanyContributions()
        {
            var contributions = await _companyContributionService.GetAllCompanyContributionAsync();
            return Ok(contributions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyContributionDto>> GetCompanyContributionById(int id)
        {
            var contribution = await _companyContributionService.GetCompanyContributionByIdAsync(id);
            if (contribution == null)
            {
                return NotFound();
            }
            return Ok(contribution);
        }

        [HttpPost]
        public async Task<ActionResult<CompanyContributionDto>> CreateCompanyContribution(CreateCompanyContributionDto createDto)
        {
            var companyContribution = new CompanyContribution
            {
                Code = createDto.Code,
                ShortDescription = createDto.ShortDescription,
                LongDescription = createDto.LongDescription,
                TaxCode = createDto.TaxCode,
                Percentage = createDto.Percentage,
                IsActive = true
            };

            var createdContribution = await _companyContributionService.CreateCompanyContributionAsync(companyContribution);
            return CreatedAtAction(nameof(GetCompanyContributionById), new { id = createdContribution.CompanyContributionId }, createdContribution);
        }
    
    }
    
}
