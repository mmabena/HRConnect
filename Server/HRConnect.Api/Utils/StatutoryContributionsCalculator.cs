namespace HRConnect.Api.Utils
{
  using HRConnect.Api.Models;

  public class StatutoryContributionsCalculator
  {
    /// <summary>
    /// Calculate employee UIF Contribution from employee's monthly salary with validation that the employee portion. This is calculate as 1% of employee's monthly solution 
    /// cannot be above R8856
    /// </summary>
    /// <param name="monthlySalary"></param>
    /// <returns>An employee respective UIF contribution</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// 
    public decimal CalculateUifEmployee(decimal monthlySalary)
    {
      if (monthlySalary < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(monthlySalary), "Monthly salary cannot be negative value");
      }
      else if (monthlySalary == 0) return 0;

      decimal employeeContribution = monthlySalary * StatutoryContributionConstants.UIFEmployeeAmount;

      if (employeeContribution >= StatutoryContributionConstants.UIFCap)
      {
        employeeContribution = StatutoryContributionConstants.UIFCap;
      }
      return employeeContribution;
    }
    /// <summary>
    /// Calculate employer UIF Contribution from employee's monthly salary with validation that the employee portion. This is calculate as 1% of employee's monthly solution 
    /// cannot be above R8856
    /// </summary>
    /// <param name="monthlySalary"></param>
    /// <returns>An employee respective UIF contribution</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// 
    public decimal CalculateUifEmployer(decimal monthlySalary)
    {
      if (monthlySalary < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(monthlySalary), "Monthly salary cannot be negative value");
      }
      else if (monthlySalary == 0) return 0;

      decimal employerContribution = monthlySalary * StatutoryContributionConstants.UIFEmployeeAmount;

      if (employerContribution >= StatutoryContributionConstants.UIFCap)
      {
        employerContribution = StatutoryContributionConstants.UIFCap;
      }
      return employerContribution;
    }
    /// <summary>
    /// Calculates total UIF amount as a sum of employer's and employee's contributions
    /// </summary>
    /// <param name="monthlySalary"></param>
    /// <returns>Employee and Employer's UIF calculated amounts</returns>
    public (decimal employeeAmount, decimal employerAmount) CalculateUif(decimal monthlySalary)
    {
      decimal employee = CalculateUifEmployee(monthlySalary);
      decimal employer = CalculateUifEmployer(monthlySalary);
      if ((employee + employer) >= StatutoryContributionConstants.UIFCap)
      {
        employee /= 2;
        employer /= 2;
      }
      return (employee, employer);
    }
    /// <summary>
    /// Calculate Skills Development Levy as 1% of employee's montly salary. Unlike UIF 
    /// amount, this amount has no cap amount
    /// </summary>
    /// <param name="monthlySalary"></param>
    /// <returns>Calculated SDL Amount</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public decimal CalculateSdlAmount(decimal monthlySalary)
    {
      if (monthlySalary < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(monthlySalary), "Monthly salary cannot be negative value");
      }
      else if (monthlySalary == 0) return 0;

      return monthlySalary * StatutoryContributionConstants.SDLAmount;
    }
  }
}