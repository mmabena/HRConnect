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

        /// <summary>
        /// Retrieves all Medical Aid dependents from the system.
        /// </summary>
        /// <returns>
        /// An IActionResult containing a list of Medical Aid dependents.
        /// </returns>
        [HttpGet]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> GetAllMedicalAidDependents()
        {
            var dependents =
                await _medicalDependentService.GetAllMedicalAidDependentsAsync();

            return Ok(dependents);
        }

        /// <summary>
        /// Retrieves a Medical Aid dependent by their dependent ID.
        /// </summary>
        /// <param name="dependentId">The dependent ID.</param>
        /// <returns>
        /// An IActionResult containing the Medical Aid dependent.
        /// </returns>
        [HttpGet("{dependentId}")]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> GetMedicalAidDependentById(string dependentId)
        {
            var dependent =
                await _medicalDependentService.GetMedicalAidDependentsByIdAsync(dependentId);

            return Ok(dependent);
        }

        /// <summary>
        /// Retrieves all Medical Aid dependents associated with a specific employee.
        /// </summary>
        /// <param name="employeeId">The employee ID.</param>
        /// <returns>
        /// An IActionResult containing a list of Medical Aid dependents associated with the employee.
        /// </returns>
        [HttpGet("employee/{employeeId}")]
        [Authorize(Roles = "SuperUser")]
        public async Task<IActionResult> GetMedicalAidDependentsByEmployeeId(string employeeId)
        {
            var dependents =
                await _medicalDependentService.GetMedicalAidDependentsByEmployeeIdAsync(employeeId);

            return Ok(dependents);
        }
        /// <summary>
        /// Validates the Medical Aid dependent information for a specific employee.
        /// </summary>
        /// <param name="employeeId">The employee ID associated with the dependent.</param>
        /// <param name="medicalAidDependentDto">The Medical Aid dependent model to be validated.</param>
        /// <returns>
        /// An IActionResult containing the validated Medical Aid dependent information.
        /// </returns>
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

        /// <summary>
        /// Creates a new Medical Aid dependent for a specific employee.
        /// </summary>
        /// <param name="employeeId">The employee ID associated with the dependent.</param>
        /// <param name="medicalAidDependentDto">The Medical Aid dependent model containing the dependent's details.</param>
        /// <returns>
        /// An IActionResult containing the created Medical Aid dependent.
        /// </returns>
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