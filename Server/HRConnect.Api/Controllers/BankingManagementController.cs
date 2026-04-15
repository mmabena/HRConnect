using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HRConnect.Api.Services;
using HRConnect.Api.DTOs.BankingDetails;

namespace HRConnect.Api.Controllers
{
    [Route("api/banking-details")]
    [ApiController]
    public class BankingManagementController : ControllerBase
    {
        private readonly IBankingDetailService _bankingDetailService;

        public BankingManagementController(IBankingDetailService bankingDetailService)
        {
            _bankingDetailService = bankingDetailService;
        }

        [HttpPost("CreateBankingDetails")]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> CreateBankingDetails([FromBody] CreateBankingDetailDto dto)
        {
            try
            {
                var result = await _bankingDetailService.CreateBankingDetailsAsync(dto);
                return CreatedAtAction(nameof(GetBankingDetails), new { employeeId = result.BankingDetailsId }, result);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while creating banking details.");
            }
        }

        [HttpPut("{EmployeeId}")]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> UpdateBankingDetails(string EmployeeId, [FromBody] UpdateBankingDetailDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("Request body cannot be null.");

                var result = await _bankingDetailService.UpdateBankingDetailsAsync(EmployeeId, dto);

                if (result == null)
                    return NotFound($"Banking details not found for employee ID: {EmployeeId}");

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    message = "An error occurred while updating banking details.",
                    error = ex.Message,
                    detail = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("{EmployeeId}")]
        public async Task<IActionResult> GetBankingDetails([FromRoute] string EmployeeId)
        {
            try
            {
                var result = await _bankingDetailService.GetBankingDetailsAsync(EmployeeId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}