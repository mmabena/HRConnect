namespace HRConnect.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using HRConnect.Api.Services;
    using HRConnect.Api.Models.CompanyContributions;
    using HRConnect.Api.Interfaces;

    [Route("api/employeeCompanyContribution")]
    [ApiController]
    public class EmployeeCompanyContributionController : ControllerBase
    {
        private readonly IEmployeeCompanyContributionService _service;

        public EmployeeCompanyContributionController(IEmployeeCompanyContributionService service)
        {
            _service = service;
        }

        [HttpGet("payrun/{payRunId}")]
        public async Task<ActionResult<List<EmployeeCompanyContribution>>> GetByPayRunId(int payRunId)
        {
            var result = await _service.GetByPayRunIdAsync(payRunId);

            if (result == null || result.Count == 0)
                return NotFound("No contribution records found for this pay run");

            return Ok(result);
        }
    }
}