namespace HRConnect.Api.Mappers
{
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Services;
  using HRConnect.Api.Models;
    using System.Xml;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides mapping methods between TaxDeduction entities and DTOs.
    /// </summary>
    public static class TaxDeductionMapper
  {
    /// <summary>
    /// Maps a TaxDeduction entity to a TaxDeductionDto.
    /// </summary>
    /// <param name="entity">The TaxDeduction entity.</param>
    /// <returns>A mapped TaxDeductionDto.</returns>
    public static TaxDeductionDto ToDto(TaxDeduction entity)
    {
      if (entity == null)
      {
        throw new ArgumentException(nameof(entity));
      }

      return new TaxDeductionDto
      {
        Id = entity.Id,
        TaxYear = entity.TaxYear,
        Remuneration = entity.Remuneration,
        AnnualEquivalent = entity.AnnualEquivalent,
        TaxUnder65 = entity.TaxUnder65,
        Tax65To74 = entity.Tax65To74,
        TaxOver75 = entity.TaxOver75
      };
    }
    /// <summary>
    /// Maps a <see cref="CreateTaxDeductionDto"> to a TaxDeduction entity
    /// </summary>
    /// <param name="dto">The creation DTO</param>
    /// <returns>a new TaxDeduction entity</returns>
    public static TaxDeduction ToEntity(CreateTaxDeductionDto dto)
    {
      if (dto == null)
      {
        throw new ArgumentException(nameof(dto));
      }

      return new TaxDeduction
      {
        TaxYear = dto.TaxYear,
        Remuneration = dto.Remuneration,
        AnnualEquivalent = dto.AnnualEquivalent,
        TaxUnder65 = dto.TaxUnder65,
        Tax65To74 = dto.Tax65To74,
        TaxOver75 = dto.TaxOver75
      };
    }
    /// <summary>
    /// Updates an existing TaxDeduction entity using an UpdateTaxDeductionDto.
    /// </summary>
    /// <param name="entity">The existing entity.</param>
    /// <param name="dto">The update DTO.</param>
    public static void UpdateEntity(TaxDeduction entity, UpdateTaxDeductionDto dto)
    {
      if (entity == null)
      {
        throw new ArgumentException(nameof(entity));
      }

      if (dto == null)
      {
        throw new ArgumentException(nameof(dto));
      }
      
      entity.Remuneration = dto.Remuneration;
      entity.AnnualEquivalent = dto.AnnualEquivalent;
      entity.TaxUnder65 = dto.TaxUnder65;
      entity.Tax65To74 = dto.Tax65To74;
      entity.TaxOver75 = dto.TaxOver75;
    }
  }
}