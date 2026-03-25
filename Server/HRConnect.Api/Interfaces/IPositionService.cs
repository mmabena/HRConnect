namespace HRConnect.Api.Interfaces
{
    using HRConnect.Api.DTOs.Position;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IPositionService
    {
        Task<List<PositionDto>> GetAllPositionsAsync();

        Task<PositionDto?> GetPositionByIdAsync(int id);

        Task<PositionDto?> GetPositionByTitleAsync(string title);

        Task<PositionDto> AddPositionAsync(CreatePositionDto createPositionDto);

        Task<PositionDto?> UpdatePositionAsync(int id, UpdatePositionDto updatePositionDto);

    }
}
