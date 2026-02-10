namespace HRConnect.Api.Mappers
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Models;

  /// <summary>
  /// Provides mapping methods between TaxTableUpload entities and DTOs
  /// </summary>
  public static class TaxTableUploadMapper
  {
    /// <summary>
    /// Maps a <see cref="TaxTableUpload"/> entity to a DTO.
    /// </summary>
    /// <param name="entity">The tax table upload entity.</param>
    /// <returns>A mapped <see cref="TaxTableUploadDto"/>.</returns>
    public static TaxTableUploadDto ToDto(TaxTableUpload entity)
    {
      if (entity == null)
        throw new ArgumentNullException(nameof(entity));

      return new TaxTableUploadDto
      {
        Id = entity.Id,
        TaxYear = entity.TaxYear,
        FileName = entity.FileName,
        FileUrl = entity.FileUrl
      };
    }
  }
}