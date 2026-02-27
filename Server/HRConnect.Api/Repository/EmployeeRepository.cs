namespace HRConnect.Api.Repository
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using HRConnect.Api.Interfaces;
  using HRConnect.Api.Models;
  using HRConnect.Api.Data;
  using HRConnect.Api.DTOs.Employee;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore.Storage;
  /// <summary>
  /// Employee Repository file responsible for all Employee data access operations.
  /// Acts as a bridge between the database context and the service layer.
  /// </summary>
  public class EmployeeRepository : IEmployeeRepository
  {
    private readonly ApplicationDBContext _context;
    public EmployeeRepository(ApplicationDBContext context)
    {
      _context = context;
    }
    /// <summary>
    /// Retrieves all Employees from the database.
    /// </summary>
    /// <returns> A List of all Employees</returns>
    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
      return await _context.Employees
            .Include(e => e.Position)
            .ToListAsync();
    }
    /// <summary>
    /// Creates a new Employee in the database.
    /// </summary>
    /// <param name="employeeModel">The employee model to create employee </param>
    /// <returns>The Created employee as objects, or null if inputs are invalid or duplicated</returns>
    public async Task<Employee> CreateEmployeeAsync(Employee employeeModel)
    {
      await _context.Employees.AddAsync(employeeModel);
      await _context.SaveChangesAsync();
      return employeeModel;
    }
    /// <summary>
    /// Creates a new Employee in the database.
    /// </summary>
    /// <param name="EmployeeId">The employee Id </param>
    /// <param name="EmployeeDto">The updated employee data </param>
    /// <returns>The Updated employee as objects, or null if inputs are invalid or duplicated</returns>
    public async Task<Employee?> UpdateEmployeeAsync(Employee employeeModel)
    {
      _context.Employees.Update(employeeModel);
      await _context.SaveChangesAsync();
      return employeeModel;
    }
    /// <summary>
    /// Retrieves a single employee by their Employee Id.
    /// </summary>
    /// <param name="EmployeeId">The employee identifier</param>
    /// <returns>The employee with the same Employee Id provided as EmployeeDTODto object or NotFound if the Employee Id does not exist</returns>
    public async Task<Employee?> GetEmployeeByIdAsync(string employeeId)
    {
      return await _context.Employees
              .Include(e => e.Position)
              .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
    }
    /// <summary>
    /// Retrieves all employee IDs that start with a given prefix.
    /// Used for Employee ID auto-generation logic. 
    /// </summary>
    /// <param name="prefix">The employee prefix</param>
    /// <returns>All employees with the provided prefix</returns>
    public async Task<List<string>> GetAllEmployeeIdsWithPrefix(string prefix)
    {
      return await _context.Employees
              .Where(e => e.EmployeeId.StartsWith(prefix))
              .Select(e => e.EmployeeId)
              .ToListAsync();
    }
    /// <summary>
    /// Deletes employee in the database by their Employee Id.
    /// </summary>
    /// <param name="EmployeeId">The employee identifier</param>
    /// <returns>true; if employee exists, false; if employee doesn't exist</returns>
    public async Task<bool> DeleteEmployeeAsync(string employeeId)
    {
      var existingEmployee = await _context.Employees
          .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

      if (existingEmployee == null)
        return false;

      _context.Employees.Remove(existingEmployee);
      await _context.SaveChangesAsync();
      return true;
    }
    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <returns>The database transaction object</returns>
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
      return await _context.Database.BeginTransactionAsync();
    }
    /// <summary>
    /// Retrieves a single employee by their email.
    /// </summary>
    /// <param name="email">The employee email</param>
    /// <returns>The employee object if found, null otherwise</returns>
    public async Task<Employee?> GetEmployeeByEmailAsync(string email)
    {
      return await _context.Employees
          .FirstOrDefaultAsync(e => e.Email == email);
    }
    /// <summary>
    /// Retrieves a single employee by their email excluding a specific EmployeeId.
    /// Used for uniqueness checks during updates.
    /// </summary>
    /// <param name="email">The employee email</param>
    /// <param name="EmployeeId">The employee identifier to exclude</param>
    /// <returns>The employee object if found, null otherwise</returns>
    public async Task<Employee?> GetEmployeeByEmailAsync(string email, string employeeId)
    {
      return await _context.Employees
          .FirstOrDefaultAsync(e => e.Email == email && e.EmployeeId != employeeId);
    }
    /// <summary>
    /// Retrieves a single employee by their ID number.
    /// </summary>
    /// <param name="idNumber">The employee ID number</param>
    /// <returns>The employee object if found, null otherwise</returns>
    public async Task<Employee?> GetEmployeeByIdNumberAsync(string idNumber)
    {
      return await _context.Employees
          .FirstOrDefaultAsync(e => e.IdNumber == idNumber);
    }
    /// <summary>
    /// Retrieves a single employee by their ID number excluding a specific EmployeeId.
    /// Used for uniqueness checks during updates.
    /// </summary>
    /// <param name="idNumber">The employee ID number</param>
    /// <param name="EmployeeId">The employee identifier to exclude</param>
    public async Task<Employee?> GetEmployeeByIdNumberAsync(string idNumber, string employeeId)
    {
      return await _context.Employees
          .FirstOrDefaultAsync(e => e.IdNumber == idNumber && e.EmployeeId != employeeId);
    }
    /// <summary>
    /// Retrieves a single employee by their Tax Number.
    /// </summary>
    /// <param name="taxNumber">The employee tax number</param>
    /// <returns>The employee object if found, null otherwise</returns>
    public async Task<Employee?> GetEmployeeByTaxNumberAsync(string taxNumber)
    {
      return await _context.Employees
          .FirstOrDefaultAsync(e => e.TaxNumber == taxNumber);
    }
    /// <summary>
    /// Retrieves a single employee by their Tax Number excluding a specific EmployeeId.
    /// Used for uniqueness checks during updates.
    /// </summary>
    /// <param name="taxNumber">The employee tax number</param>
    /// <param name="EmployeeId">The employee identifier to exclude</param>
    /// <returns>The employee object if found, null otherwise</returns>
    public async Task<Employee?> GetEmployeeByTaxNumberAsync(string taxNumber, string employeeId)
    {
      return await _context.Employees
          .FirstOrDefaultAsync(e => e.TaxNumber == taxNumber && e.EmployeeId != employeeId);
    }
    /// <summary>
    /// Retrieves a single employee by their Passport Number.
    /// </summary>
    /// <param name="passportNumber">The employee passport number</param>
    /// <returns>The employee object if found, null otherwise</returns>
    public async Task<Employee?> GetEmployeeByPassportAsync(string passportNumber)
    {
      return await _context.Employees
          .FirstOrDefaultAsync(e => e.PassportNumber == passportNumber);
    }
    /// <summary>
    /// Retrieves a single employee by their Passport Number excluding a specific EmployeeId.
    /// Used for uniqueness checks during updates.
    /// </summary>
    /// <param name="passportNumber">The employee passport number</param>
    /// <param name="EmployeeId">The employee identifier to exclude</param>
    /// <returns>The employee object if found, null otherwise</returns>
    public async Task<Employee?> GetEmployeeByPassportAsync(string passportNumber, string employeeId)
    {
      return await _context.Employees
          .FirstOrDefaultAsync(e => e.PassportNumber == passportNumber && e.EmployeeId != employeeId);
    }
    /// <summary>
    /// Retrieves a single employee by their Contact Number.
    /// </summary>
    /// <param name="contactNumber">The employee contact number</param>
    /// <returns>The employee object if found, null otherwise</returns>
    public async Task<Employee?> GetEmployeeByContactNumberAsync(string contactNumber)
    {
      return await _context.Employees
          .FirstOrDefaultAsync(e => e.ContactNumber == contactNumber);
    }
    /// <summary>
    /// Retrieves a single employee by their Contact Number excluding a specific EmployeeId.
    /// Used for uniqueness checks during updates.
    /// </summary>
    /// <param name="contactNumber">The employee contact number</param>
    /// <param name="EmployeeId">The employee identifier to exclude</param>
    /// <returns>The employee object if found, null otherwise</returns>
    public async Task<Employee?> GetEmployeeByContactNumberAsync(string contactNumber, string EmployeeId)
    {
      return await _context.Employees
          .FirstOrDefaultAsync(e => e.ContactNumber == contactNumber && e.EmployeeId != EmployeeId);
    }
  }
}