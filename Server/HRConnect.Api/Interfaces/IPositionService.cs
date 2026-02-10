namespace HRConnect.Api.Services
{
    using HRConnect.Api.DTOs.Position;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IPositionService
    {
        Task<IEnumerable<ReadPositionDto>> GetAllPositionsAsync();

        Task<ReadPositionDto?> GetPositionByIdAsync(int id);

        Task<ReadPositionDto?> GetPositionByTitleAsync(string title);

        Task<ReadPositionDto> CreatePositionAsync(CreatePositionDto createPositionDto);

        Task<ReadPositionDto?> UpdatePositionAsync(int id, UpdatePositionDto updatePositionDto);

        Task<bool> DeletePositionAsync(int id);
    }
}
