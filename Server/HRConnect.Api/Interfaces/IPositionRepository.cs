namespace HRConnect.Api.Interfaces
{
    using HRConnect.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
    public interface IPositionRepository
    {
        Task<List<Position>> GetAllPositionsAsync();

        Task<Position?> GetPositionByIdAsync(int id);

        Task<Position?> GetPositionByTitleAsync(string title);

        Task<bool> TitleExistsAsync(string title, int excludeId = 0);

        Task<Position> AddPositionAsync(Position position);

        Task<Position?> UpdatePositionAsync(int id, Position position);

    }
}
