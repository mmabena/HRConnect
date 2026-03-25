namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.DTOs.Employee.Pension;
  using HRConnect.Api.Interfaces.Pension;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  [Route("api/employeepensionenrollment")]
  [ApiController]
  [Authorize(Roles = "SuperUser")]
  public class EmployeePensionEnrollmentController(IEmployeePensionEnrollmentService employeePensionEnrollmentService) : ControllerBase
  {
    private readonly IEmployeePensionEnrollmentService _employeePensionEnrollmentService = employeePensionEnrollmentService;

    ///<summary>
    ///Enroll an employee into a pension plan
    ///</summary>
    ///<param name="employeePensionEnrollmentAddDto">Employee's Pension Enrollment Request Data Transfer Object</param>
    ///<returns>
    ///IActionResult with employees pension enrollment
    ///</returns>
    [HttpPost("enroll")]
    public async Task<IActionResult> EnrollEmployeesPension(EmployeePensionEnrollmentAddDto employeePensionEnrollmentAddDto)
    {
      EmployeePensionEnrollmentDto enrolledPensionPlan = await _employeePensionEnrollmentService.
        AddEmployeePensionEnrollmentAsync(employeePensionEnrollmentAddDto);

      return Ok(enrolledPensionPlan);
    }

    ///<summary>
    ///Get all pension plans
    ///</summary>
    ///<returns>
    ///IActionResult with list of all pension plans
    ///</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllEmployeePensionEnrollments()
    {
      List<EmployeePensionEnrollmentDto> enrolledPensionPlans = await _employeePensionEnrollmentService.GetAllEmployeePensionEnrollementsAsync();
      return Ok(enrolledPensionPlans);
    }

    ///<summary>
    ///Get employee's latest pension enrollment by employee id
    ///</summary>
    ///<param name="employeeId">Employee's Id</param>
    ///<returns>
    ///IActionResult with employee's latest pension enrollment
    ///</returns>
    [HttpGet]
    [Route("employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeePensionEnrollementById([FromRoute] string employeeId)
    {
      EmployeePensionEnrollmentDto? employeesEnrolledPensionPlan = await _employeePensionEnrollmentService.
        GetEmployeePensionEnrollementByIdAsync(employeeId);

      return Ok(employeesEnrolledPensionPlan);
    }

    ///<summary>
    ///Get all pension plans by payroll run id
    ///</summary>
    ///<param name="payrollRunId">Pay Roll Run Id</param>
    ///<returns>
    ///IActionResult with list of all pension plans by payroll run id
    ///</returns>
    [HttpGet]
    [Route("employeepensionenrollment/{payrollRunId}")]
    public async Task<IActionResult> GetPensionEnrollementsByPayRollRunId(int payrollRunId)
    {
      List<EmployeePensionEnrollmentDto> enrolledPensionPlans = await _employeePensionEnrollmentService.
        GetPensionEnrollementsByPayRollRunIdAsync(payrollRunId);

      return Ok(enrolledPensionPlans);
    }

    ///<summary>
    ///Update employee's pension enrollment
    ///</summary>
    ///<param name="employeePensionEnrollmentUpdateDto">Employee's Pension Enrollment Update Request Data Transfer Object</param>
    ///<returns>
    ///IActionResult with employee's updated pension enrollment
    ///</returns>
    [HttpPut]
    public async Task<IActionResult> UpdateEmployeePensionEnrollment(EmployeePensionEnrollmentUpdateDto employeePensionEnrollmentUpdateDto)
    {
      EmployeePensionEnrollmentDto? updatedPensionEnrollment = await _employeePensionEnrollmentService.
        UpdateEmployeePensionEnrollementAsync(employeePensionEnrollmentUpdateDto);

      return Ok(updatedPensionEnrollment);
    }
  }
}
