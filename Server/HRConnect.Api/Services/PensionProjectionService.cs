namespace HRConnect.Api.Services
{
  using System.Globalization;
  using HRConnect.Api.DTOs.Employee.Pension;
  using HRConnect.Api.Interfaces.PensionProjection;
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils;

  public class PensionProjectionService : IPensionProjectionService
  {
    private readonly int PENSION_PROJECTION_AGE_LIMIT = 65;
    private readonly decimal MAX_PENSIONCONTRIBUTION_PERCENTAGE = (decimal)27.5 / 100;
    private static readonly decimal MAX_MONTHLYCONTRIBUTION = 29166.66M;
    private readonly float SALARYINCREASE_PERCENTAGE = 0.05f;
    private readonly float PENSIONGROWTH_PRECENTAGE = 0.06f;
    private decimal[][]? _monthlyContributions;
    private decimal[][]? _voluntaryContributions;
    private decimal _totalProjectedSavings = 0.00M;
    private int _yearWhenMonthlyContributionWasCapped;
    private int _monthWhenMonthlyContributionWasCapped;

    ///<summary>
    ///Project pension
    ///</summary>
    ///<param name="pensionProjectRequestDto">Pension Project Request Data Transfer Object</param>
    ///<returns>
    ///Pension Project Result data transfer object with pension savings details
    ///</returns>
    public PensionProjectionResultDto ProjectPension(PensionProjectionRequestDto pensionProjectRequestDto)
    {
      int currentAge = CalculateAge.UsingDOB(pensionProjectRequestDto.DOB);
      if (ValidEmployeeForPensionProjection(currentAge, pensionProjectRequestDto.EmploymentStatus) != "")
      {
        return new PensionProjectionResultDto
        {
          WarningMessage = ValidEmployeeForPensionProjection(
            currentAge,
            pensionProjectRequestDto.EmploymentStatus)
        };
      }

      if (pensionProjectRequestDto.VoluntaryContribution != 0)
      {
        if (ValidVoluntaryContribution(pensionProjectRequestDto.VoluntaryContribution, PensionOption.GetPensionPercentage(pensionProjectRequestDto.SelectedPensionPercentage),
          pensionProjectRequestDto.Salary) != "")
        {
          return new PensionProjectionResultDto
          {
            WarningMessage = ValidVoluntaryContribution(
                pensionProjectRequestDto.VoluntaryContribution,
                PensionOption.GetPensionPercentage(pensionProjectRequestDto.SelectedPensionPercentage),
                pensionProjectRequestDto.Salary
              )
          };
        }
      }

      PensionProjectionResultDto pensionProjectionResultDto = new()
      {
        CurrentAge = currentAge,
        YearsUntilRetirement = (DateTime.Today.Month < pensionProjectRequestDto.DOB.Month) ? PENSION_PROJECTION_AGE_LIMIT - currentAge - 1
        : PENSION_PROJECTION_AGE_LIMIT - currentAge,
        TotalProjectedSavings = 0
      };

      CalculateMonthlyContributions(pensionProjectRequestDto.DOB, pensionProjectionResultDto.YearsUntilRetirement, pensionProjectRequestDto.Salary,
        pensionProjectRequestDto.SelectedPensionPercentage, pensionProjectRequestDto.VoluntaryContribution, pensionProjectRequestDto.VoluntaryContributionFrequency);
      pensionProjectionResultDto.MonthlyContribution = _monthlyContributions;
      pensionProjectionResultDto.MonthlyVoluntaryContribution = _voluntaryContributions;
      pensionProjectionResultDto.TotalProjectedSavings = _totalProjectedSavings;

      if ((_yearWhenMonthlyContributionWasCapped != 0) && (_monthWhenMonthlyContributionWasCapped != 0))
      {
        pensionProjectionResultDto.WarningMessage = $"From {_yearWhenMonthlyContributionWasCapped}, " +
          $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(_monthWhenMonthlyContributionWasCapped)} your monthly contribution was " +
          $"capped at the maximum allowable monthly amount (R29166,66). ";
      }

      pensionProjectionResultDto.LumpSum = CalculateRetirementLumpSum(pensionProjectionResultDto.TotalProjectedSavings);
      pensionProjectionResultDto.MonthlyIncomeAfterRetirement = CalculateMonthlyIncomeAfterRetirement(pensionProjectionResultDto.TotalProjectedSavings);

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
    private string ValidEmployeeForPensionProjection(int currentAge, EmploymentStatus EmploymentStatus)
    {
      string warningMessage = "";

      if (currentAge >= PENSION_PROJECTION_AGE_LIMIT)
      {
        warningMessage += "You have reached retirement age — projections not available.";
      }
      if (EmploymentStatus != EmploymentStatus.Permanent)
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

    ///<summary>
    ///Initialize monthly salary contributions array according to years until retirement 
    ///</summary>
    ///<param name="yearsUntilRetirement">Number of years until retirement starting today</param>
    private void InitializeMonthlyContributionArray(int yearsUntilRetirement)
    {
      _monthlyContributions = new decimal[yearsUntilRetirement + 1][];
      for (int i = 0; i < yearsUntilRetirement + 1; i++)
      {
        _monthlyContributions[i] = new decimal[12];
      }
    }

    ///<summary>
    ///Initialize monthly voluntary contributions array according to years until retirement 
    ///</summary>
    ///<param name="yearsUntilRetirement">Number of years until retirement starting today</param>
    private void InitializeVoluntaryContributionArray(int yearsUntilRetirement)
    {
      _voluntaryContributions = new decimal[yearsUntilRetirement + 1][];
      for (int i = 0; i < yearsUntilRetirement + 1; i++)
      {
        _voluntaryContributions[i] = new decimal[12];
      }
    }

    ///<summary>
    ///Caculates total projected pension savings 
    ///</summary>
    ///<param name="dateOfBirth">Date of birth</param>
    ///<param name="yearsUntilRetirement">Number of years until retirement starting today</param>
    ///<param name="salary">Employee Salary</param>
    ///<param name="selectedPercentage">Pension percentage deducted from employee salary</param>
    ///<param name="voluntaryContribution">Bonus contribution to the employee's salary percentage monthly contribution</param>
    ///<param name="voluntaryContributionFrequency">Is the voluntary contribution a single payment or frequent monthly payment</param>
    private void CalculateMonthlyContributions(DateTime dateOfBirth, int yearsUntilRetirement, decimal salary, int selectedPercentage, decimal voluntaryContribution = 0.00M,
      ContributionFrequency voluntaryContributionFrequency = 0)
    {
      DateTime targetDate = dateOfBirth.AddYears(PENSION_PROJECTION_AGE_LIMIT);
      DateTime today = DateTime.Today;
      decimal empSalary = salary;
      InitializeMonthlyContributionArray(yearsUntilRetirement);

      if (voluntaryContributionFrequency == ContributionFrequency.OnceOff)
      {
        _totalProjectedSavings += voluntaryContribution;
      }
      else if (voluntaryContributionFrequency == ContributionFrequency.Permanent)
      {
        InitializeVoluntaryContributionArray(yearsUntilRetirement);
      }

      int determineWhenIsApril = DetermineWhenIsApril();
      int currentMonth = (today.Day < 25) ? today.Month : today.Month + 1;
      bool stopYearLoop = false;
      for (int year = 0; year < yearsUntilRetirement + 1 && !stopYearLoop; year++)
      {
        for (int month = currentMonth; month <= 12; month++)
        {
          if ((month == determineWhenIsApril + today.Month) && currentMonth == today.Month)
          {
            empSalary = Math.Round(empSalary * (decimal)(SALARYINCREASE_PERCENTAGE + 1), 2);
          }
          else if (month == 4)
          {
            empSalary = Math.Round(empSalary * (decimal)(SALARYINCREASE_PERCENTAGE + 1), 2);
          }

          decimal monthlyContribution = Math.Round(empSalary * (decimal)PensionOption.GetPensionPercentage(selectedPercentage), 2);

          if (voluntaryContributionFrequency == ContributionFrequency.Permanent)
          {
            decimal voluntaryContributionAfterSalaryIncrease = CalculateExcessAmountFromVoluntaryContribution(monthlyContribution, voluntaryContribution);
            _monthlyContributions[year][month - 1] = PensionMonthlyContributionCap(monthlyContribution + voluntaryContributionAfterSalaryIncrease);
            _voluntaryContributions[year][month - 1] = voluntaryContributionAfterSalaryIncrease;
            _totalProjectedSavings += Math.Round(PensionMonthlyContributionCap(monthlyContribution + voluntaryContribution), 2);
          }
          else
          {
            _monthlyContributions[year][month - 1] = PensionMonthlyContributionCap(monthlyContribution);
            _totalProjectedSavings += Math.Round(monthlyContribution, 2);
          }

          if ((_monthlyContributions[year][month - 1] == MAX_MONTHLYCONTRIBUTION) && (_yearWhenMonthlyContributionWasCapped == 0) && (_monthWhenMonthlyContributionWasCapped == 0))
          {
            _yearWhenMonthlyContributionWasCapped = today.Year + year;
            _monthWhenMonthlyContributionWasCapped = month;
          }

          if (month == 12)
          {
            _totalProjectedSavings = Math.Round(_totalProjectedSavings * (decimal)(PENSIONGROWTH_PRECENTAGE + 1), 2);
            currentMonth = 1;
            continue;
          }

          if (year == yearsUntilRetirement && month == targetDate.Month)
          {
            stopYearLoop = true;
            break;
          }
        }
      }
    }
  }
}