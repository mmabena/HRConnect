namespace HRConnect.Api.Utils.MedicalOption.ValidationHelpers
{
  using DTOs.MedicalOption;
  using Interfaces;
  using Models;

  public class MedicalOptionVariantComprehensiveValidations
  {
    #region Variant Comprehensive Validation

    /// <summary>
    /// Comprehensive validation that processes each variant individually with full validation,
    /// then performs cross-variant validation
    /// </summary>
    public static async Task<BulkValidationResult> ValidateAllVariantsComprehensiveAsync(
      int categoryId,
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
      IMedicalOptionRepository repository,
      List<MedicalOption> dbData)
    {
      var result = new BulkValidationResult();

      try
      {
        // Basic period validation (global)
        if (!MedicalOptionBasicValidations.ValidateUpdatePeriod(DateTime.Now))
        {
          result.IsValid = false;
          result.ErrorMessage = "Updates can only be performed between November and December";
          return result;
        }

        if (dbData.Count == 0)
        {
          result.IsValid = false;
          result.ErrorMessage = $"No medical options found for category ID: {categoryId}";
          return result;
        }

        // Group by variant using your existing factory
        var variantGroups = MedicalOptionValidator.GroupOptionsByVariant(bulkUpdateDto, dbData);
        
        // Validate each variant individually with ALL validations
        foreach (var variantGroup in variantGroups)
        {
          var variantValidation = await MedicalOptionSingleVariantValidations.ValidateSingleVariantWithAllRulesAsync(
            variantGroup.Key, 
            variantGroup.Value, 
            bulkUpdateDto, 
            repository,
            categoryId, dbData);

          if (!variantValidation.IsValid)
          {
            result.IsValid = false;
            result.ErrorMessage = $"Variant '{variantGroup.Key}' validation failed: " +
                                  $"{variantValidation.ErrorMessage}";
            return result;
          }
        }
      }
      catch (Exception ex)
      {
        result.IsValid = false;
        result.ErrorMessage = $"Comprehensive validation error: {ex.Message}";
      }

      return result;
    }

    #endregion
  }
}