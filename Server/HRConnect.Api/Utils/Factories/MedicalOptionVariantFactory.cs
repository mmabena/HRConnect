namespace HRConnect.Api.Utils.Factories
{
  using HRConnect.Api.Models;
  using HRConnect.Api.Utils;
  using HRConnect.Api.Utils.Enums;

  public static class MedicalOptionVariantFactory
  {
    // TODO : Detailed Documentation
    /// <summary>
    /// Factory method to get enum variant by type name
    /// </summary>
    /// <param name="enumTypeName">
    /// The name of the enum type (e.g., "Choice", "Essential", "Vital")
    /// </param>
    /// <param name="categoryName">The category name to map to enum variant</param>
    /// <returns>The enum variant value or null if not found</returns>
    public static object? GetVariantByName(string enumTypeName, string categoryName)
    {
      // If enumTypeName is actually the enum value, get its type name
      var actualTypeName = enumTypeName.StartsWith("HRConnect", StringComparison.Ordinal) 
        ? enumTypeName.Split('.').Last() 
        : enumTypeName;
    
      return actualTypeName switch
      {
        "Choice" => MedicalOptionEnumMapper.GetEnumVariant<Choice>(categoryName),
        "Essential" => MedicalOptionEnumMapper.GetEnumVariant<Essential>(categoryName),
        "Vital" => MedicalOptionEnumMapper.GetEnumVariant<Vital>(categoryName),
        "Alliance" => MedicalOptionEnumMapper.GetEnumVariant<Alliance>(categoryName),
        "Double" => MedicalOptionEnumMapper.GetEnumVariant<Double>(categoryName),
        _ => throw new ArgumentException($"Unknown enum type: {actualTypeName}")
      };
    }
    
    /// <summary>
    /// Enhanced factory method with error handling and logging
    /// </summary>
    /// <param name="option">The medical option to analyze</param>
    /// <returns>Tuple containing (CategoryName, VariantName, FilterName)</returns>
    public static (string CategoryName, string VariantName, string FilterName) 
      GetVariantInfoSafe(MedicalOption option)
    {
      try
      {
        var trimmedOptionName = MedicalOptionUtils.OptionNameFormatter(option.MedicalOptionName);
        var (categoryName, enumType) = MedicalOptionEnumMapper
          .GetCategoryInfoFromVariant(trimmedOptionName);

        if (categoryName == null || enumType == null)
        {
          return (string.Empty, string.Empty, string.Empty);
        }

        var variant = GetVariantByName(enumType.Name, categoryName);
        var variantName = variant?.ToString() ?? string.Empty;
        string altFilterName = null;
        if (categoryName.Contains("Choice"))
        {
          altFilterName = categoryName;
          return (categoryName, variantName, altFilterName);
        }
        
        if (variantName == "" || variantName == null)
        {
          variantName = trimmedOptionName.Split(' ')[1].TrimEnd();
        }
        
        var filterName = option.MedicalOptionCategory?.MedicalOptionCategoryName + " " + 
                         variantName;
        
        return (categoryName, variantName, filterName);
      }
      catch (Exception ex)
      {
        // Log error if needed
        Console.WriteLine($"Error getting variant info for {option.MedicalOptionName}:" +
                          $"{ex.Message}");
        return (string.Empty, string.Empty, string.Empty);
      }
    }
    
    /// <summary>
    /// Factory method to get variant info by category name directly
    /// </summary>
    /// <param name="option">The medical option to analyze</param>
    /// <param name="categoryName">The category name to use for variant extraction</param>
    /// <returns>Tuple containing (CategoryName, VariantName, FilterName)</returns>
    public static (string CategoryName, string VariantName, string FilterName) 
      GetVariantInfoByCategory(MedicalOption option, string categoryName)
    {
      try
      {
        var variant = GetVariantByName(GetEnumTypeFromCategory(categoryName), categoryName);
        var variantName = variant?.ToString() ?? string.Empty;
        
        var filterName = option.MedicalOptionCategory?.MedicalOptionCategoryName + " " + 
                         variantName;
        
        return (categoryName, variantName, filterName);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error getting variant info for {option.MedicalOptionName}: " +
                          $"{ex.Message}");
        return (string.Empty, string.Empty, string.Empty);
      }
    }

    /// <summary>
    /// Helper method to determine enum type from category name
    /// </summary>
    private static string GetEnumTypeFromCategory(string categoryName)
    {
      return categoryName switch
      {
        var name when name.Contains("Choice") => "Choice",
        var name when name.Contains("Essential") => "Essential",
        var name when name.Contains("Vital") => "Vital",
        _ => throw new ArgumentException($"Cannot determine enum type for category:{categoryName}")
      };
    }
  }
}