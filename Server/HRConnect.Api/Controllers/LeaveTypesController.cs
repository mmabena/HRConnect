namespace HRConnect.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using HRConnect.Api.DTOs;
    using HRConnect.Api.Interfaces;
    [ApiController]
    [Route("api/leave-types")]
    //[Authorize(Roles = "SuperUser")]
    public class LeaveTypesController : ControllerBase
    {
        private readonly ILeaveTypeManagementService _service;

        public LeaveTypesController(ILeaveTypeManagementService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _service.GetLeaveTypesAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
            => Ok(await _service.GetLeaveTypeByIdAsync(id));

        [HttpPost]
        public async Task<IActionResult> Create(CreateLeaveTypeRequest request)
            => Ok(await _service.CreateLeaveTypeAsync(request));

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateLeaveTypeRequest request)
            => Ok(await _service.UpdateLeaveTypeAsync(id, request));
    }
}