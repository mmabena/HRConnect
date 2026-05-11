namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.DTOs.Payroll.Deduction;
  using HRConnect.Api.Interfaces.Payroll.Deduction;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  [ApiController]
  [Route("api/employeededuction")]
  [Authorize(Roles = "SuperUser")]
  public class EmployeeDeductionController(IEmployeeDeductionService employeeDeductionService) : ControllerBase
  {
    private readonly IEmployeeDeductionService _employeeDeductionService = employeeDeductionService;

    ///<summary>
    ///Add new employee deduction  
    ///</summary>
    ///<param name="employeeDeductionAddDto">Employee deduction add request data transfer object</param>
    ///<returns>
    ///Added employee deduction
    ///</returns>
    [HttpPost]
    public async Task<IActionResult> AddEmployeeDeduction(EmployeeDeductionAddDto employeeDeductionAddDto)
    {
      EmployeeDeductionDto addedEmployeeDeduction = await _employeeDeductionService.AddAsync(employeeDeductionAddDto);
      return Ok(addedEmployeeDeduction);
    }

    ///<summary>
    ///Retreive all employee deductions 
    ///</summary>
    ///<returns>
    ///A list of all employee deductions
    ///</returns>
    [HttpGet]
    public async Task<IActionResult> GetEmployeeDeductions()
    {
      List<EmployeeDeductionDto> employeeDeductionsDto = await _employeeDeductionService.GetAllAsync();
      return Ok(employeeDeductionsDto);
    }

    ///<summary>
    ///Retreive all employee deductions for current payroll run 
    ///</summary>
    ///<param name="payrollRunId">Payroll run Id</param>
    ///<returns>
    ///A list of employee deductions for current payroll run
    ///</returns>
    [HttpGet]
    [Route("payrollrun/{payrollRunId}")]
    public async Task<IActionResult> GetEmployeeDeductionForCurrentPayrollRun([FromRoute] int payrollRunId)
    {
      List<EmployeeDeductionDto> employeeDeductionsDto = await _employeeDeductionService.GetByPayrollRunIdAsync(payrollRunId);
      return Ok(employeeDeductionsDto);
    }

    ///<summary>
    ///Retreive all employee's deductions 
    ///</summary>
    ///<param name="employeeId">Employee Id</param>
    ///<returns>
    ///A list of employee's deductions
    ///</returns>
    [HttpGet]
    [Route("employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeeDeductions([FromRoute] string employeeId)
    {
      List<EmployeeDeductionDto> employeeDeductionsDto = await _employeeDeductionService.GetByEmployeeIdAsync(employeeId);
      return Ok(employeeDeductionsDto);
    }

    ///<summary>
    ///Retreive the employee's deduction for current payroll run and are not locked
    ///</summary>
    ///<param name="employeeId">Employee Id</param>
    ///<returns>
    ///A list of employee's deduction for current payroll run and are not locked
    ///</returns>
    [HttpGet]
    [Route("employee/notlocked/{employeeId}")]
    public async Task<IActionResult> GetEmployeeDeductionsForCurrentPayrollRunAndNotLocked([FromRoute] string employeeId)
    {
      List<EmployeeDeductionDto> employeeDeductionsDtos = await _employeeDeductionService.GetByEmployeeIdAndIsNotLockedAsync(employeeId);
      return Ok(employeeDeductionsDtos);
    }

    ///<summary>
    ///Update employee's deduction
    ///</summary>
    ///<param name="employeeDeductionUpdateDto">Employee deduction add request data transfer object</param>
    ///<returns>
    ///Updated employee's deduction
    ///</returns>
    [HttpPut]
    public async Task<IActionResult> UpdateEmployeeDeduction(EmployeeDeductionUpdateDto employeeDeductionUpdateDto)
    {
      EmployeeDeductionDto updatedEmployeeDeduction = await _employeeDeductionService.UpdateAsync(employeeDeductionUpdateDto);
      return Ok(updatedEmployeeDeduction);
    }
  }
}
