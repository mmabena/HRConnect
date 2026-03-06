namespace HRConnect.Api.Utils.MedicalOption.ValidationHelpers
{
  using DTOs.MedicalOption;
  using Enums;
  using Enums.Mappers;
  using Interfaces;
  using Models;

  public static class MedicalOptionBasicValidations
  {
    #region Basic Validations

    /// <summary>
    /// Validates if the update operation is within the allowed period (November-December)
    /// </summary>
    /// <param name="dateTime"></param>
    public static bool ValidateUpdatePeriod(DateTime dateTime)
    {
      // dateTime = DateTime.Now;
      return DateRangeUpdatePeriod.CategoryOptionsUpdatePeriod.Contains(dateTime);
    }

    /// <summary>
    /// Validates entity count matches between database and payload
    /// </summary>
    public static bool ValidateEntityCount(int dbEntityCount, int bulkPayloadCount)
    {
      return dbEntityCount == bulkPayloadCount;
    }

    /// <summary>
    /// Validates if all medical option IDs in the payload exist in the database
    /// </summary>
    public static bool ValidateAllIdsExistAsync(
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
      IMedicalOptionRepository repository, List<MedicalOption> dbData)
    {
      var existingIds = dbData.Select(o => o.MedicalOptionId).ToHashSet();

      foreach (var entity in bulkUpdateDto)
      {
        if (!existingIds.Contains(entity.MedicalOptionId))
          // In-memory check
          return false;
      }

      return true;
    }

    /// <summary>
    /// Validates if all entities belong to the specified category
    /// </summary>
    public static bool ValidateAllIdsInCategoryAsync(
      IReadOnlyCollection<UpdateMedicalOptionVariantsDto> bulkUpdateDto,
      int categoryId,
      IMedicalOptionRepository repository, List<MedicalOption> dbData)
    {
      var categoryOptions = dbData
        .Where(o => o.MedicalOptionCategoryId == categoryId).ToHashSet();

      foreach (var entity in bulkUpdateDto)
      {
        if (!categoryOptions.Any(o => o.MedicalOptionId == entity.MedicalOptionId))
          // In-memory check
          return false;
      }

      return true;
    }

    #endregion

  }
}