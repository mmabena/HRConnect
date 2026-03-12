namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.DTOs.Employee.Pension;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Interfaces.Pension;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  [Route("api/employeepensionenrollment")]
  [ApiController]
  [Authorize(Roles = "SuperUser")]
  public class EmployeePensionEnrollmentController(IEmployeePensionEnrollmentService employeePensionEnrollmentService) : ControllerBase
  {
    private readonly IEmployeePensionEnrollmentService _employeePensionEnrollmentService = employeePensionEnrollmentService;

    [HttpPost("enroll")]
    public async Task<IActionResult> EnrollEmployeesPension(EmployeePensionEnrollmentAddDto employeePensionEnrollmentAddDto)
    {
      EmployeePensionEnrollmentDto enrolledPensionPlan = await _employeePensionEnrollmentService.
        AddEmployeePensionEnrollmentAsync(employeePensionEnrollmentAddDto);

      return Ok(enrolledPensionPlan);
    }
    [HttpGet]
    public async Task<IActionResult> GetAllEmployeePensionEnrollments()
    {
      List<EmployeePensionEnrollmentDto> enrolledPensionPlans = await _employeePensionEnrollmentService.GetAllEmployeePensionEnrollementsAsync();
      return Ok(enrolledPensionPlans);
    }

    [HttpGet]
    [Route("employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeePensionEnrollementById([FromRoute] string employeeId)
    {
      EmployeePensionEnrollmentDto? employeesEnrolledPensionPlan = await _employeePensionEnrollmentService.
        GetEmployeePensionEnrollementByIdAsync(employeeId);

      return Ok(employeesEnrolledPensionPlan);
    }

    [HttpGet]
    [Route("employeepensionenrollment/{payrollRunId}")]
    public async Task<IActionResult> GetPensionEnrollementsByPayRollRunId(int payrollRunId)
    {
      List<EmployeePensionEnrollmentDto> enrolledPensionPlans = await _employeePensionEnrollmentService.
        GetPensionEnrollementsByPayRollRunIdAsync(payrollRunId);

      return Ok(enrolledPensionPlans);
    }
    public async Task<IActionResult> UpdateEmployeePensionEnrollment(EmployeePensionEnrollmentUpdateDto employeePensionEnrollmentUpdateDto)
    {
      EmployeePensionEnrollmentDto? updatedPensionEnrollment = await _employeePensionEnrollmentService.
        UpdateEmployeePensionEnrollementAsync(employeePensionEnrollmentUpdateDto);

      return Ok(updatedPensionEnrollment);
    }

    /*Task<bool> DeleteEmployeePensionEnrollementAsync(EmployeePensionEnrollmentAddDto employeePensionEnrollmentDto);*/
  }
}
