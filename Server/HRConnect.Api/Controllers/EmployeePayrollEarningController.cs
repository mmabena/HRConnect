namespace HRConnect.Api.Controllers
{
  using HRConnect.Api.DTOs.Payroll.Earning;
  using HRConnect.Api.Interfaces.Payroll.Earning;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  [Route("api/employeepayrollearning")]
  [ApiController]
  [Authorize(Roles = "SuperUser")]
  public class EmployeePayrollEarningController(IEmployeePayrollEarningService employeePayrollEarningService) : ControllerBase
  {
    private readonly IEmployeePayrollEarningService _employeePayrollEarningService = employeePayrollEarningService;

    ///<summary>
    /// Adds a new employee payroll earning
    ///</summary>
    ///<param name="employeePayrollEarningAddDto">The employee payroll earning add data transfer object</param>
    ///<returns>
    ///The added employee payroll earning
    ///</returns>
    [HttpPost]
    public async Task<IActionResult> AddEmployeePayrollEarning(EmployeePayrollEarningAddDto employeePayrollEarningAddDto)
    {
      EmployeePayrollEarningDto employeePayrollEarningDto = await _employeePayrollEarningService.AddAsync(employeePayrollEarningAddDto);
      return Ok(employeePayrollEarningDto);
    }

    ///<summary>
    ///Retrieves all employee payroll earnings
    ///</summary>
    ///<returns>
    ///A list of employee payroll earnings
    ///</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllEmployeePayrollEarnings()
    {
      List<EmployeePayrollEarningDto> employeePayrollEarnings = await _employeePayrollEarningService.GetAllAsync();
      return Ok(employeePayrollEarnings);
    }

    ///<summary>
    ///Retrieves employee payroll earnings by employee id   
    ///</summary>
    ///<param name="employeeId">Employee Id</param>
    ///<returns>
    ///A list of employee payroll earnings for the specified employee id
    ///</returns>
    [HttpGet]
    [Route("employee/{employeeId}")]
    public async Task<IActionResult> GetEmployeePayrollEarningsByEmployeeIdAsync([FromRoute] string employeeId)
    {
      List<EmployeePayrollEarningDto> employeePayrollEarnings = await _employeePayrollEarningService.GetByEmployeeIdAsync(employeeId);
      return Ok(employeePayrollEarnings);
    }

    ///<summary>
    ///Retrieves employee payroll earnings by employee id that are not locked
    ///</summary>
    ///<param name="employeeId">Employee Id</param>
    ///<returns>
    ///A list of employee payroll earnings for the specified employee id that are not locked
    ///</returns>
    [HttpGet]
    [Route("employee/notlocked/{employeeId}")]
    public async Task<IActionResult> GetEmployeePayrollEarningsByEmployeeIdAndNotLocked([FromRoute] string employeeId)
    {
      List<EmployeePayrollEarningDto> employeePayrollEarnings = await _employeePayrollEarningService.GetByEmployeeIdAndIsNotLockedAsync(employeeId);
      return Ok(employeePayrollEarnings);
    }

    ///<summary>
    ///Retrieves employee payroll earnings by payroll run id
    ///</summary>
    ///<param name="payrollRunId">Payroll Run Id</param>
    ///<returns>
    ///A list of employee payroll earnings for the specified payroll run id
    ///</returns>
    [HttpGet]
    [Route("payrollrunid/{payrollRunId}")]
    public async Task<IActionResult> GetEmployeePayrollEarningsByPayrollRunIdAsync([FromRoute] int payrollRunId)
    {
      List<EmployeePayrollEarningDto> employeePayrollEarnings = await _employeePayrollEarningService.GetByPayrollRunIdAsync(payrollRunId);
      return Ok(employeePayrollEarnings);
    }

    ///<summary>
    ///Retrieves employee payroll earnings by tax code 
    ///</summary>
    ///<param name="taxCode">Tax Code</param>
    ///<returns>
    ///A list of employee payroll earnings for the specified tax code
    ///</returns>
    [HttpGet]
    [Route("taxcode/{taxCode}")]
    public async Task<IActionResult> GetEmployeePayrollEarningsByTaxCodeAsync([FromRoute] int taxCode)
    {
      List<EmployeePayrollEarningDto> employeePayrollEarnings = await _employeePayrollEarningService.GetByTaxCodeAsync(taxCode);
      return Ok(employeePayrollEarnings);
    }

    ///<summary>
    ///Retrieves employee payroll earnings by payroll earning id 
    ///</summary>
    ///<param name="payrollEarningId">Payroll Earning Id</param>
    ///<returns>
    ///A list of employee payroll earnings for the specified payroll earning id
    ///</returns>
    [HttpGet]
    [Route("payrollearningid/{payrollEarningId}")]
    public async Task<IActionResult> GetEmployeePayrollEarningsByPayrollEarningIdAsync([FromRoute] string payrollEarningId)
    {
      List<EmployeePayrollEarningDto> employeePayrollEarnings = await _employeePayrollEarningService.GetByPayrollEarningIdAsync(payrollEarningId);
      return Ok(employeePayrollEarnings);
    }

    ///<summary>
    ///Updates an employee payroll earning
    ///</summary>
    ///<param name="employeePayrollEarningUpdateDto">Employee Payroll Earning Update DTO</param>
    ///<returns>
    ///The updated employee payroll earning
    ///</returns>
    [HttpPut]
    public async Task<IActionResult> UpdateEmployeePayrollEarning(EmployeePayrollEarningUpdateDto employeePayrollEarningUpdateDto)
    {
      EmployeePayrollEarningDto employeePayrollEarningDto = await _employeePayrollEarningService.UpdateAsync(employeePayrollEarningUpdateDto);
      return Ok(employeePayrollEarningDto);
    }
  }
}
