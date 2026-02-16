namespace HRConnect.Api.Utils
{
  using DTOs.MedicalOption;
  using HRConnect.Api.Utils.Enums;
  using HRConnect.Api.Models;
  using HRConnect.Api.Interfaces;
  using Microsoft.Identity.Client;

  public static class MedicalOptionUtils
  {
    public static string OptionNameFormatter(string optionName)
    {
      return optionName.Replace(optionName.Last().ToString(), "").TrimEnd();
    }

    // Fetching the filtered option variant
    public static async Task<List<MedicalOptionSalaryDto>> GetFilteredOptionVariant(
      object? filterName, object? categoryName, IMedicalOptionRepository medicalOptionRepository, 
      MedicalOption? option)
    {
      if (!categoryName.ToString().Contains("Choice"))
      {
        filterName = categoryName.ToString() + " " + filterName.ToString();
      }
      else
      {
        filterName = filterName.ToString();
      }
      
      var trimmedDownOptions = (await medicalOptionRepository
          .GetAllMedicalOptionsUnderCategoryVarientAsync(filterName.ToString()))
        .Where(opt => opt.MedicalOptionCategoryId == option.MedicalOptionCategoryId)
        .Select(opt => new MedicalOptionSalaryDto
        {
          MedicalOptionID = opt!.MedicalOptionId,
          MedicalOptionName = opt.MedicalOptionName,
          MedicalOptionCategoryId = opt.MedicalOptionCategoryId,
          SalaryBracketMin = opt?.SalaryBracketMin,
          SalaryBracketMax = opt?.SalaryBracketMax
        }).ToList();

      if (trimmedDownOptions != null)
      {
        return trimmedDownOptions;
      }
      else
      {
        return null;
      }
    }

    public static bool ValidateSalaryBracketUpdateRequest(
      List<MedicalOptionSalaryDto> trimmedDownOptions, int optionId,
      UpdateMedicalOptionSalaryBracketRequestDto requestDto, MedicalOption? option)
    {
      //Validate Salary brackets to check for overlaps
      var overlappingSalaryBrackets = trimmedDownOptions
        .Where(o => o.MedicalOptionID != optionId) // excludes current option
        .FirstOrDefault(o => !(requestDto.SalaryBracketMax <
          option.SalaryBracketMin || requestDto.SalaryBracketMax > option.SalaryBracketMin));

      if (overlappingSalaryBrackets != null)
      {
        throw new ArgumentException(
          $"Salary bracket {requestDto.SalaryBracketMin}-{requestDto.SalaryBracketMax} " +
          $"overlaps with existing option '{overlappingSalaryBrackets.MedicalOptionName}' " +
          $"({overlappingSalaryBrackets.SalaryBracketMin}-{overlappingSalaryBrackets
            .SalaryBracketMax})"
        );
        return false; //unreachable code - TODO: Fix return type
      }
      else
      {
        //Logger.LogInformation("Salary bracket is valid");
        return true;
      }
    }
  } 
}