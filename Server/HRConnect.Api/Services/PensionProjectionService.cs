namespace HRConnect.Api.Services
{
  using HRConnect.Api.Interfaces.Finance;
  using HRConnect.Api.DTOs.Employee.Pension;
  using HRConnect.Api.Utils;

  public class PensionProjectionService : IPensionProjectionService
  {
    private readonly int PENSION_PROJECTION_AGE_LIMIT = 65;
    private readonly string EMPLOYEE_EMPLOYEMENT_TYPE = "Permanent";
    private readonly decimal MAX_PENSIONCONTRIBUTION_PERCENTAGE = (decimal)27.5 / 100;
    private readonly decimal MAX_MONTHLYCONTRIBUTION = Math.Round((decimal)350000 / 12, 2);
    private readonly float SALARYINCREASE_PERCENTAGE = 0.05f;
    private readonly float PENSIONGROWTH_PRECENTAGE = 0.06f;
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
      //int yearsUntilRetirement = targetDate.Year - today.Year;
      int numberOfMonthsUntilRetirement = (pensionProjectionResultDto.YearsUntilRetirement * 12) + targetDate.Month - today.Month;
      if (today.Day > 25)
      {
        numberOfMonthsUntilRetirement--;
      }
      int monthsUntilEndOfTheYear = 12 - today.Month;

      //pensionProjectionResultDto.MonthlyContribution = new decimal[pensionProjectionResultDto.YearsUntilRetirement + 1, 12];
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

        pensionProjectionResultDto.TotalProjectedSavings += PensionMonthlyContributionCap(empSalary * (decimal)PensionOption.options[pensionProjectRequestDto.SelectedPensionPercentage - 1]);
        pensionProjectionResultDto.MonthlyContribution[0][i - 1] = PensionMonthlyContributionCap(empSalary * (decimal)PensionOption.options[pensionProjectRequestDto.SelectedPensionPercentage - 1]);

        if (i == (monthsUntilEndOfTheYear + today.Month))
        {
          pensionProjectionResultDto.TotalProjectedSavings *= Math.Round((decimal)(PENSIONGROWTH_PRECENTAGE + 1), 2);
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

        pensionProjectionResultDto.TotalProjectedSavings += PensionMonthlyContributionCap(empSalary * (decimal)PensionOption.options[pensionProjectRequestDto.SelectedPensionPercentage - 1]);
        pensionProjectionResultDto.MonthlyContribution[year][monthlyIndex] = PensionMonthlyContributionCap(empSalary * (decimal)PensionOption.options[pensionProjectRequestDto.SelectedPensionPercentage - 1]);

        if (monthlyIndex == 11)
        {
          pensionProjectionResultDto.TotalProjectedSavings *= Math.Round((decimal)(PENSIONGROWTH_PRECENTAGE + 1), 2);
          year++;
          monthlyIndex = 0;
          continue;
        }
        monthlyIndex++;
      }

      return pensionProjectionResultDto;
    }

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

    private string ValidVoluntaryContribution(decimal voluntaryContribution, float selectedPensionPercentage, decimal salary)
    {
      string warningMessage = "";
      float voluntaryContributionPercentage = (float)(voluntaryContribution / salary * 100);

      if ((voluntaryContributionPercentage + selectedPensionPercentage) > (float)MAX_PENSIONCONTRIBUTION_PERCENTAGE)
      {
        warningMessage += "Voluntary Contribution + Monthly Salary Contribution cannot exceed 27.5% of salary";
      }

      return warningMessage;
    }

    private decimal PensionMonthlyContributionCap(decimal monthlyContribution)
    {
      return (monthlyContribution > MAX_MONTHLYCONTRIBUTION) ? Math.Round(MAX_MONTHLYCONTRIBUTION, 2) : Math.Round(monthlyContribution, 2);
    }

    private static int DetermineWhenIsApril()
    {
      DateTime today = DateTime.Today;
      int april = 4;
      int monthsLeftUntilApril = today.Month <= april ? april - today.Month : 12 - today.Month + april;

      return monthsLeftUntilApril;
    }

  }
}