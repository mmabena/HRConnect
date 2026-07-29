namespace HRConnect.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HRConnect.Api.DTOs.MedicalAidDependent;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.Mappers;
    using HRConnect.Api.Models;
    using System.Globalization;
    using HRConnect.Api.Utils;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;
    [ApiController]
    [Route("api/medicalDependent")]
    public class MedicalAidDependentController : ControllerBase
    {
        private readonly IMedicalAidDependentService _medicalDependentService;

        public MedicalAidDependentController(IMedicalAidDependentService medicalDependentService)
        {
            _medicalDependentService = medicalDependentService;
            
        }

        // GET: api/MedicalAidDependent
        [HttpGet]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> GetAllMedicalAidDependents()
        {
            var dependents =
                await _medicalDependentService.GetAllMedicalAidDependentsAsync();

            return Ok(dependents);
        }

        // GET: api/MedicalAidDependent/{dependentId}
        [HttpGet("{dependentId}")]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> GetMedicalAidDependentById(string dependentId)
        {
            var dependent =
                await _medicalDependentService.GetMedicalAidDependentsByIdAsync(dependentId);

            return Ok(dependent);
        }

        // GET: api/MedicalAidDependent/employee/ARM001
        [HttpGet("employee/{employeeId}")]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> GetMedicalAidDependentsByEmployeeId(string employeeId)
        {
            var dependents =
                await _medicalDependentService.GetMedicalAidDependentsByEmployeeIdAsync(employeeId);

            return Ok(dependents);
        }

        [HttpPost("employee/{employeeId}/validate")]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> ValidateMedicalAidDependent(
            string employeeId,
            [FromBody] CreateMedicalAidDependentRequestDTO medicalAidDependentDto)
        {
            var validated =
                await _medicalDependentService.ValidateMedicalAidDependentAsync(employeeId, medicalAidDependentDto);

            return Ok(validated);
        }

        // POST: api/MedicalAidDependent/employee/ARM001
        [HttpPost("employee/{employeeId}")]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> CreateMedicalAidDependent(
            string employeeId,
            [FromBody] CreateMedicalAidDependentRequestDTO medicalAidDependentDto)
        {
            var created =
                await _medicalDependentService.CreateMedicalAidDependentAsync(employeeId, medicalAidDependentDto);

            return CreatedAtAction(
                nameof(GetMedicalAidDependentById),
                new { dependentId = created.DependentId },
                created);
        }

        /*
        // PUT: api/MedicalAidDependent/{dependentId}
        [HttpPut("{dependentId}")]
        public async Task<IActionResult> Update(
            string dependentId,
            UpdateMedicalAidDependentRequestDTO dto)
        {
            var updated =
                await _medicalDependentService.UpdateMedicalAidDependentAsync(dependentId, dto);

            return Ok(updated);
        }

        // DELETE: api/MedicalAidDependent/{dependentId}
        [HttpDelete("{dependentId}")]
        public async Task<IActionResult> Delete(string dependentId)
        {
            await _medicalDependentService.DeleteMedicalAidDependentAsync(dependentId);

            return NoContent();
        }
        */

    }
}