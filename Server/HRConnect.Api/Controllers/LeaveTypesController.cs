namespace HRConnect.Api.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;

    [ApiController]
    [Route("api/leave-types")]
    [Authorize(Roles = "SuperUser")]
    public class LeaveTypesController : ControllerBase
    {
        private readonly ILeaveTypeManagementService _service;

        public LeaveTypesController(ILeaveTypeManagementService service)
        {
            _service = service;
        }
        /// <summary>
        /// Handles the HTTP GET request to retrieve all leave types,
        /// by calling the GetLeaveTypesAsync method of the ILeaveTypeManagementService to fetch the list of leave types from the database,
        /// and then returns the list of leave types in the response body, allowing clients to view all available leave types through the API.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetLeaveTypesAsync());
        /// <summary>
        /// Handles the HTTP GET request to retrieve a specific leave type by its ID,
        /// by accepting the leave type ID as a route parameter,
        /// and then returns the leave type in the response body, allowing clients to view specific leave type details through the API.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
            => Ok(await _service.GetLeaveTypeByIdAsync(id));
        /// <summary>        
        /// /// Handles the HTTP POST request to create a new leave type,
        /// by accepting a CreateLeaveTypeRequest DTO in the request body, which contains the necessary information for creating a new leave type,
        /// and then calls the CreateLeaveTypeAsync method of the ILeaveTypeManagementService to create the new leave type in the database,
        /// returning the created leave type in the response body, 
        /// allowing clients to add new leave types through the API and receive feedback on the created leave type details.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLeaveTypeRequest request)
        {
            var result = await _service.CreateLeaveTypeAsync(request);
            return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
        }
        /// <summary>
        /// Handles the HTTP PUT request to update an existing leave type,
        /// by accepting the leave type ID as a route parameter and an UpdateLeaveTypeRequest DTO in the request body, 
        /// which contains the updated information for the leave type,
        /// and then calls the UpdateLeaveTypeAsync method of the ILeaveTypeManagementService to update the leave type in the database,
        /// returning the updated leave type in the response body, 
        /// allowing clients to modify existing leave types through the API and receive feedback on the updated leave type details.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateLeaveTypeRequest request)
            => Ok(await _service.UpdateLeaveTypeAsync(id, request));
        
        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployeesWithLeave()
        {
            var result = await _service.GetAllEmployeesWithLeaveAsync();
            return Ok(result);
        }
        [HttpGet("employees/{employeeId}")]
        public async Task<IActionResult> GetEmployeeWithLeave(string employeeId)
        {
            var result = await _service.GetEmployeeWithLeaveByIdAsync(employeeId);

            if (result == null)
                return NotFound("Employee not found");

            return Ok(result);
        }

    }
}