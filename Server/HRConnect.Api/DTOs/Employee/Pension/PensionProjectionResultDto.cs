namespace HRConnect.Api.DTOs.Employee.Pension
{
  public class PensionProjectionResultDto
  {
    public int CurrentAge { get; set; }
    public int YearsUntilRetirement { get; set; }
    public decimal[][]? MonthlyContribution { get; set; } //= new double[YearsUntilRetirement];
    public decimal TotalProjectedSavings { get; set; }
    public decimal LumpSum { get; set; }
    public decimal MonthlyIncomeAfterRetirement { get; set; }
    public decimal InitialVoluntaryContribution { get; set; }
    public decimal[][]? MonthlyVoluntaryContribution { get; set; }
    public string WarningMessage { get; set; } = string.Empty;
  }
}