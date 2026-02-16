namespace HRConnect.Api.Utils
{
  using HRConnect.Api.Models;

  public class PayrollDeductionsCalculator
  {
    public decimal CalculateUifEmployee(decimal monthlySalary)
    {
      if (monthlySalary < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(monthlySalary), "Monthly salary cannot be negative value");
      }
      else if (monthlySalary == 0) return 0;

      decimal employeeContribution = monthlySalary * DeductionConstants.UIFEmployeeAmount;
      //In the case empployee contribution is a above the R17 712 cap
      if (employeeContribution >= DeductionConstants.UIFCap)
      {
        employeeContribution = DeductionConstants.UIFCap;
      }
      return employeeContribution;
    }
    public decimal CalculateUifEmployer(decimal monthlySalary)
    {
      if (monthlySalary < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(monthlySalary), "Monthly salary cannot be negative value");
      }
      else if (monthlySalary == 0) return 0;

      decimal employerContribution = monthlySalary * DeductionConstants.UIFEmployeeAmount;
      //In the case empployee contribution is a above the R17 712 cap
      if (employerContribution >= DeductionConstants.UIFCap)
      {
        employerContribution = DeductionConstants.UIFCap;
      }
      return employerContribution;
    }
    public (decimal employeeAmount, decimal employerAmount) CalculateUif(decimal monthlySalary)
    {
      decimal employee = CalculateUifEmployee(monthlySalary);
      decimal employer = CalculateUifEmployer(monthlySalary);
      if ((employee + employer) >= DeductionConstants.UIFCap)
      {
        employee /= 2;
        employer /= 2;
      }
      return (employee, employer);
    }
    public decimal CalculateSdlAmount(decimal monthlySalary)
    {
      if (monthlySalary < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(monthlySalary), "Monthly salary cannot be negative value");
      }
      else if (monthlySalary == 0) return 0;

      return monthlySalary * DeductionConstants.SDLAmount;
    }
  }
}