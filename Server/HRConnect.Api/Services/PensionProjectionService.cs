namespace HRConnect.Api.Services
{
  using System.Globalization;
  using HRConnect.Api.DTOs.Employee.Pension;
  using HRConnect.Api.Interfaces.Finance;
  using HRConnect.Api.Utils;

  public class PensionProjectionService : IPensionProjectionService
  {
    private readonly int PENSION_PROJECTION_AGE_LIMIT = 65;
    private readonly string EMPLOYEE_EMPLOYEMENT_TYPE = "Permanent";
    private readonly decimal MAX_PENSIONCONTRIBUTION_PERCENTAGE = (decimal)27.5 / 100;
    private static readonly decimal MAX_MONTHLYCONTRIBUTION = 29166.66M;
    private readonly float SALARYINCREASE_PERCENTAGE = 0.05f;
    private readonly float PENSIONGROWTH_PRECENTAGE = 0.06f;
    ///<summary>
    ///Project pension
    ///</summary>
    ///<param name="pensionProjectRequestDto">Pension Project Request Data Transfer Object</param>
    ///<returns>
    ///Pension Project Result data transfer object with pension savings details
    ///</returns>
    public PensionProjectionResultDto ProjectPension(PensionProjectionRequestDto pensionProjectRequestDto)
    {
      if (ValidEmployeeForPensionProjection(pensionProjectRequestDto.DOB, pensionProjectRequestDto.EmploymentEmploymentStatus) != "")
      {
        return new PensionProjectionResultDto
        {
          WarningMessage = ValidEmployeeForPensionProjection(
            pensionProjectRequestDto.DOB,
            pensionProjectRequestDto.EmploymentEmploymentStatus)
        };
      }

      if (pensionProjectRequestDto.VoluntaryContribution != 0)
      {
        if (ValidVoluntaryContribution(pensionProjectRequestDto.VoluntaryContribution, PensionOption.options[pensionProjectRequestDto.SelectedPensionPercentage - 1], pensionProjectRequestDto.Salary) != "")
        {
          return new PensionProjectionResultDto
          {
            WarningMessage = ValidVoluntaryContribution(
                pensionProjectRequestDto.VoluntaryContribution,
                PensionOption.options[pensionProjectRequestDto.SelectedPensionPercentage - 1],
                pensionProjectRequestDto.Salary
              )
          };
        }
      }

      PensionProjectionResultDto pensionProjectionResultDto = new()
      {
        CurrentAge = CalculateAge.UsingDOB(pensionProjectRequestDto.DOB),
        YearsUntilRetirement = PENSION_PROJECTION_AGE_LIMIT - CalculateAge.UsingDOB(pensionProjectRequestDto.DOB),
        TotalProjectedSavings = 0
      };

      DateTime targetDate = pensionProjectRequestDto.DOB.AddYears(PENSION_PROJECTION_AGE_LIMIT);
      DateTime today = DateTime.Today;
      pensionProjectionResultDto.YearsUntilRetirement = targetDate.Year - today.Year;
      int yearWhenMonthlyContributionWasCapped = 0;
      int monthWhenMonthlyContributionWasCapped = 0;


      int numberOfMonthsUntilRetirement = (pensionProjectionResultDto.YearsUntilRetirement * 12) + targetDate.Month - today.Month;
      if (today.Day > 25)
      {
        numberOfMonthsUntilRetirement--;
      }
      int monthsUntilEndOfTheYear = 12 - today.Month;

      if (pensionProjectRequestDto.VoluntaryContributionFrequency == ContributionFrequency.OnceOff)
      {
        pensionProjectionResultDto.InitialVoluntaryContribution = pensionProjectRequestDto.VoluntaryContribution;
        pensionProjectionResultDto.TotalProjectedSavings += pensionProjectRequestDto.VoluntaryContribution;
      }
      else if (pensionProjectRequestDto.VoluntaryContributionFrequency == ContributionFrequency.Permanent)
      {
        pensionProjectionResultDto.InitialVoluntaryContribution = pensionProjectRequestDto.VoluntaryContribution;
        pensionProjectionResultDto.MonthlyVoluntaryContribution = new decimal[pensionProjectionResultDto.YearsUntilRetirement + 1][];
        for (int i = 0; i < pensionProjectionResultDto.YearsUntilRetirement + 1; i++)
        {
          pensionProjectionResultDto.MonthlyVoluntaryContribution[i] = new decimal[12];
        }
      }

      pensionProjectionResultDto.MonthlyContribution = new decimal[pensionProjectionResultDto.YearsUntilRetirement + 1][];
      for (int i = 0; i < pensionProjectionResultDto.YearsUntilRetirement + 1; i++)
      {
        pensionProjectionResultDto.MonthlyContribution[i] = new decimal[12];
      }
      decimal empSalary = pensionProjectRequestDto.Salary;

      for (int i = today.Month; i <= monthsUntilEndOfTheYear + today.Month; i++)
      {
        if (i == (DetermineWhenIsApril() + today.Month))
        {
          empSalary *= (decimal)(SALARYINCREASE_PERCENTAGE + 1);
        }

        if (pensionProjectRequestDto.VoluntaryContributionFrequency == ContributionFrequency.Permanent)
        {
          decimal voluntaryContributionAfterSalaryIncrease = CalculateExcessAmountFromVoluntaryContribution(empSalary * (decimal)PensionOption.options[pensionProjectRequestDto.SelectedPensionPercentage - 1], pensionProjectRequestDto.VoluntaryContribution);
          pensionProjectionResultDto.MonthlyVoluntaryContribution[0][i - 1] = voluntaryContributionAfterSalaryIncrease;
          pensionProjectionResultDto.TotalProjectedSavings += Math.Round(PensionMonthlyContributionCap((empSalary * (decimal)PensionOption.options[pensionProjectRequestDto.SelectedPensionPercentage - 1]) + voluntaryContributionAfterSalaryIncrease), 2);
          pensionProjectionResultDto.MonthlyContribution[0][i - 1] = PensionMonthlyContributionCap((empSalary * (decimal)PensionOption.options[pensionProjectRequestDto.SelectedPensionPercentage - 1]) + voluntaryContributionAfterSalaryIncrease);
        }
        else
        {
          pensionProjectionResultDto.TotalProjectedSavings += Math.Round(PensionMonthlyContributionCap(empSalary * (decimal)PensionOption.options[pensionProjectRequestDto.SelectedPensionPercentage - 1]), 2);
          pensionProjectionResultDto.MonthlyContribution[0][i - 1] = PensionMonthlyContributionCap(empSalary * (decimal)PensionOption.options[pensionProjectRequestDto.SelectedPensionPercentage - 1]);
        }

        if (i == (monthsUntilEndOfTheYear + today.Month))
        {
          pensionProjectionResultDto.TotalProjectedSavings = Math.Round(pensionProjectionResultDto.TotalProjectedSavings * (decimal)(PENSIONGROWTH_PRECENTAGE + 1), 2);
        }

        if ((pensionProjectionResultDto.MonthlyContribution[0][i - 1] == MAX_MONTHLYCONTRIBUTION) && (yearWhenMonthlyContributionWasCapped == 0) && (monthWhenMonthlyContributionWasCapped == 0))
        {
          yearWhenMonthlyContributionWasCapped = today.Year;
          monthWhenMonthlyContributionWasCapped = i;
        }
      }

      numberOfMonthsUntilRetirement -= monthsUntilEndOfTheYear;
      int year = 1;
      int monthlyIndex = 0;

      for (int i = 0; i < numberOfMonthsUntilRetirement; i++)
      {
        if (monthlyIndex == 3)
        {
          empSalary *= (decimal)(SALARYINCREASE_PERCENTAGE + 1);
        }

        if (pensionProjectRequestDto.VoluntaryContributionFrequency == ContributionFrequency.Permanent)
        {
          decimal voluntaryContributionAfterSalaryIncrease = CalculateExcessAmountFromVoluntaryContribution(empSalary * (decimal)PensionOption.options[pensionProjectRequestDto.SelectedPensionPercentage - 1], pensionProjectRequestDto.VoluntaryContribution);
          pensionProjectionResultDto.MonthlyVoluntaryContribution[year][monthlyIndex] = voluntaryContributionAfterSalaryIncrease;
          pensionProjectionResultDto.TotalProjectedSavings += Math.Round(PensionMonthlyContributionCap((empSalary * (decimal)PensionOption.options[pensionProjectRequestDto.SelectedPensionPercentage - 1]) + voluntaryContributionAfterSalaryIncrease), 2);
          pensionProjectionResultDto.MonthlyContribution[year][monthlyIndex] = PensionMonthlyContributionCap((empSalary * (decimal)PensionOption.options[pensionProjectRequestDto.SelectedPensionPercentage - 1]) + voluntaryContributionAfterSalaryIncrease);
        }
        else
        {
          pensionProjectionResultDto.TotalProjectedSavings += Math.Round(PensionMonthlyContributionCap(empSalary * (decimal)PensionOption.options[pensionProjectRequestDto.SelectedPensionPercentage - 1]), 2);
          pensionProjectionResultDto.MonthlyContribution[year][monthlyIndex] = PensionMonthlyContributionCap(empSalary * (decimal)PensionOption.options[pensionProjectRequestDto.SelectedPensionPercentage - 1]);
        }

        if ((pensionProjectionResultDto.MonthlyContribution[year][monthlyIndex] == MAX_MONTHLYCONTRIBUTION) && (yearWhenMonthlyContributionWasCapped == 0) && (monthWhenMonthlyContributionWasCapped == 0))
        {
          yearWhenMonthlyContributionWasCapped = today.Year + year;
          monthWhenMonthlyContributionWasCapped = monthlyIndex + 1;
          //pensionProjectionResultDto.WarningMessage = $"From {today.Year + year}, {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthlyIndex + 1)} your  was contribution capped at the maximum allowable monthly amount. ";
        }

        if (monthlyIndex == 11)
        {
          pensionProjectionResultDto.TotalProjectedSavings = Math.Round(pensionProjectionResultDto.TotalProjectedSavings * (decimal)(PENSIONGROWTH_PRECENTAGE + 1), 2);
          year++;
          monthlyIndex = 0;
          continue;
        }
        monthlyIndex++;
      }

      pensionProjectionResultDto.LumpSum = CalculateRetirementLumpSum(pensionProjectionResultDto.TotalProjectedSavings);
      pensionProjectionResultDto.MonthlyIncomeAfterRetirement = CalculateMonthlyIncomeAfterRetirement(pensionProjectionResultDto.TotalProjectedSavings);

      if ((yearWhenMonthlyContributionWasCapped != 0) && (monthWhenMonthlyContributionWasCapped != 0))
      {
        pensionProjectionResultDto.WarningMessage = $"From {yearWhenMonthlyContributionWasCapped}, {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(monthWhenMonthlyContributionWasCapped)} your monthly contribution was capped at the maximum allowable monthly amount (R29166,66). ";
      }

      return pensionProjectionResultDto;
    }

    ///<summary>
    ///Validate if employee is on a permament
    ///</summary>
    ///<param name="DOB">Date of Birth</param>
    ///<param name="EmploymentStatus">Employee employment status</param>
    ///<returns>
    ///Warning message to state if employee employment status is invalid
    ///</returns>
    private string ValidEmployeeForPensionProjection(DateTime DOB, string EmploymentStatus)
    {
      string warningMessage = "";
      int age = CalculateAge.UsingDOB(DOB);

      if (age >= PENSION_PROJECTION_AGE_LIMIT)
      {
        warningMessage += "You have reached retirement age — projections not available.\n";
      }
      if (EmploymentStatus != EMPLOYEE_EMPLOYEMENT_TYPE)
      {
        warningMessage += "Pension projections not available for fixed-term employees.";
      }

      return warningMessage;
    }

    ///<summary>
    ///Validate if employee salary percentage plus voluntary contribution is not more than 27.5% of employee's current salary 
    ///</summary>
    ///<param name="voluntaryContribution">Bonus contribution to the employee's salary percentage monthly contribution</param>
    ///<param name="selectedPensionPercentage">The selected percentage to contribute towards pension savings</param>
    ///<param name="salary">Employee's current salary</param>
    ///<returns>
    ///Warning message to state employee's voluntary contribution + salary monthly contribution exceeds 27.5% of employee's current salary 
    ///</returns>
    private string ValidVoluntaryContribution(decimal voluntaryContribution, float selectedPensionPercentage, decimal salary)
    {
      string warningMessage = "";
      float voluntaryContributionPercentage = (float)(voluntaryContribution / salary);

      if ((voluntaryContributionPercentage + selectedPensionPercentage) > (float)MAX_PENSIONCONTRIBUTION_PERCENTAGE)
      {
        warningMessage += "Voluntary Contribution + Monthly Salary Contribution cannot exceed 27.5% of salary";
      }

      return warningMessage;
    }

    ///<summary>
    ///Caps all monthly contributions to R29166,66 if monthly contribution >  R29166,66
    ///</summary>
    ///<param name="monthlyContribution">Employee's monthly contribution towards pension fund</param>
    ///<returns>
    ///Monthly contribution if monthly contribution <= R29166,66 else R29166,66 if monthly contribution > R29166,66
    ///</returns>
    private static decimal PensionMonthlyContributionCap(decimal monthlyContribution)
    {
      return (monthlyContribution > MAX_MONTHLYCONTRIBUTION) ? Math.Round(MAX_MONTHLYCONTRIBUTION, 2) : Math.Round(monthlyContribution, 2);
    }

    ///<summary>
    ///Determine when is April according to date time today
    ///</summary>
    ///<returns>
    ///Number of months until April
    ///</returns>
    private static int DetermineWhenIsApril()
    {
      DateTime today = DateTime.Today;
      int april = 4;
      int monthsLeftUntilApril = today.Month <= april ? april - today.Month : 12 - today.Month + april;

      return monthsLeftUntilApril;
    }

    ///<summary>
    ///Calculate the amount that will be given to employee upon retirement
    ///</summary>
    ///<param name="totalProjectedSavings">Total amount of savings in pension fund</param>
    ///<returns>
    ///30% of totalProjectedSavings
    ///</returns>
    private static decimal CalculateRetirementLumpSum(decimal totalProjectedSavings)
    {
      return Math.Round(totalProjectedSavings * (decimal)0.3f, 2);
    }

    ///<summary>
    ///Calculate the monthly income that the employee will get upon retirement for next 10 years
    ///</summary>
    ///<param name="totalProjectedSavings">Total amount of savings in pension fund</param>
    ///<returns>
    ///Monthly income that the employee will get for the next 10 years
    ///</returns>
    private static decimal CalculateMonthlyIncomeAfterRetirement(decimal totalProjectedSavings)
    {
      return Math.Round((totalProjectedSavings - CalculateRetirementLumpSum(totalProjectedSavings)) / 10 / 12, 2);
    }

    ///<summary>
    ///Calculate voluntary contribution needed due to salary increase
    ///</summary>
    ///<param name="contributionFromSalary">Employee's salary percentage to fund pension</param>
    ///<param name="voluntaryContribution">Bonus contribution to the employee's salary percentage monthly contribution</param>
    ///<returns>
    ///Reduced voluntary contribution due to salary increase
    ///</returns>
    private static decimal CalculateExcessAmountFromVoluntaryContribution(decimal contributionFromSalary, decimal voluntaryContribution)
    {
      if ((contributionFromSalary + voluntaryContribution) > MAX_MONTHLYCONTRIBUTION)
      {
        decimal excessAmountFromVoluntaryContribution = MAX_MONTHLYCONTRIBUTION - contributionFromSalary;
        decimal voluntaryContributionAfterSalaryIncrease = (excessAmountFromVoluntaryContribution > 0) ? excessAmountFromVoluntaryContribution : 0;

        return Math.Round(voluntaryContributionAfterSalaryIncrease, 2);
      }
      else
      {
        return voluntaryContribution;
      }
    }

  }
}