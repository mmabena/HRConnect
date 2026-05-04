namespace HRConnect.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using HRConnect.Api.Interfaces;
    using HRConnect.Api.DTOs.BankingDetails;
    using HRConnect.Api.Services;
    using Microsoft.AspNetCore.Authorization;

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperUser")]
    public class BankingDetailsController : ControllerBase
    {
        private readonly IBankingDetailService _service;

        public BankingDetailsController(IBankingDetailService service)
        {
            _service = service;
        }

        /// <summary>
        /// Retrieves all banking details from the database.
        /// This method calls the service layer to retrieve a list of all banking details records.
        /// </summary>
        /// <returns>A list of all banking details</returns>
    
        [HttpGet]
        public async Task<IActionResult> GetAllBankingDetails()
        {
            var result = await _service.GetAllBankingDetailsAsync();
            return Ok(result);
        }

        [HttpGet("BankBranchCodes")]
        public async Task<IActionResult> GetAllBankBranchCodes()
        {
            var result = await _service.GetAllBankBranchCodesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Retrieves the banking details of a temporary employee by their employee ID.
        /// This method accepts an employee ID as a parameter and calls the service layer to retrieve the corresponding banking details. 
        /// The service layer will handle the logic of querying the database and returning the appropriate banking details record. 
        /// If a record is found, it is returned in the response; otherwise, a NotFound result may be returned to indicate that no banking details exist for the given employee ID.
        /// </summary>
        /// <param name="employeeId">The ID of the employee whose banking details are to be retrieved</param>
        /// <returns>The banking details for the specified employee</returns>
        
        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetBankingDetailsByEmployeeId(string employeeId)
        {
            var result = await _service.GetBankingDetailsByEmployeeIdAsync(employeeId);
            return Ok(result);
        }

        /// <summary>
        /// Creates new banking details for a temporary employee. 
        /// This method accepts a CreateBankingDetailDto containing the necessary information to create a new banking details record for an employee. 
        /// The service layer will handle the creation logic, including any necessary validation and mapping from the DTO to the BankingDetail model. 
        /// Upon successful creation, the newly created banking details are returned in the response.
        /// </summary>
        /// <param name="dto">The DTO containing the banking details to create</param>
        /// <returns>The created banking details</returns>
        [HttpPost("CreateBankingDetails")]
        public async Task<IActionResult> CreateBankingDetails([FromBody] CreateBankingDetailDto dto)
        {
            var result = await _service.CreateBankingDetailsAsync(dto);
            return Ok(result);
        }


        /// <summary>
        /// Updates the banking details of a temporary employee identified by their employee ID.
        /// This method accepts an employee ID and an UpdateBankingDetailDto containing the updated banking details
        /// </summary>
        /// <param name="EmployeeId">The ID of the employee whose banking details are to be updated</param>
        /// <param name="dto">The DTO containing the updated banking details</param>
        /// <returns>The updated banking details</returns>
        [HttpPut("{EmployeeId}")]
        public async Task<IActionResult> UpdateBankingDetails(string EmployeeId, [FromBody] UpdateBankingDetailDto dto)
        {
            var result = await _service.UpdateBankingDetailsAsync(EmployeeId, dto);
            return Ok(result);
        }

       
    }
}