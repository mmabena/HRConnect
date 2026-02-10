namespace HRConnect.Api.Utils
{
  using HRConnect.Api.Models;

  public class PayrollContributionCalculator
  {
    public decimal CalculateUifEmployee(decimal monthlySalary)
    {
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
      return monthlySalary * DeductionConstants.SDLAmount;
    }
  }
}