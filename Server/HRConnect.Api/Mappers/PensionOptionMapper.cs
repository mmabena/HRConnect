namespace HRConnect.Api.Mappers
{
  using HRConnect.Api.DTOs;
  using HRConnect.Api.Models;

  public static class PensionOptionMapper
  {
    public static PensionOptionDto ToDto(this PensionOption entity)
    {
      return new PensionOptionDto
      {
        PensionOptionId = entity.PensionOptionId,
        ContributionPercentage = entity.ContributionPercentage
      };
    }

    public static PensionOption ToEntity(PensionOptionDto dto)
    {
      return new PensionOption
      {
        PensionOptionId = dto.PensionOptionId,
        ContributionPercentage = dto.ContributionPercentage
      };
    }
  }
}



