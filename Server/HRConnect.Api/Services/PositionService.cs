namespace HRConnect.Api.Services
{
 
    using System.Data;
   

    public class PositionService : IPositionService
    {
          private readonly IPositionRepository _positionRepo;

        public PositionService(IPositionRepository positionRepo)
        {
            _positionRepo = positionRepo;
        }

        public async Task<IEnumerable<ReadPositionDto>> GetAllPositionsAsync()
        {
            var positions = await _positionRepo.GetAllPositionsAsync();
            return positions.Select(p => p.ToReadPositionDto());
        }

        public async Task<ReadPositionDto?> GetPositionByIdAsync(int id)
        {
            var position = await _positionRepo.GetPositionByIdAsync(id);
            return position?.ToReadPositionDto();
        }

        private async Task ValidatePositionExistsAsync(string title)
        {
    
            if (await _positionRepo.GetPositionByTitleAsync(title) == null)
            {
                throw new KeyNotFoundException($"Position with title {title} not found.");
            }
        }
        private async Task ValidateUniqueTitleAsync(string title, int id)
        {
    if (await _positionRepo.TitleExistsAsync(title, id))
        throw new ArgumentException("Position title already exists.");
        }

        public async Task<ReadPositionDto> CreatePositionAsync(CreatePositionDto createPositionDto)
        {
            ValidateTitle(createPositionDto.Title);
            await ValidateUniqueTitleAsync(createPositionDto.Title, 0);
            await ValidateJobGradeAsync(createPositionDto.JobGradeId);
            await ValidateOccupationalLevelAsync(createPositionDto.OccupationalLevelId);

            var position = createPositionDto.ToPosition();
            var createdPosition = await _positionRepo.CreatePositionAsync(position);
            return createdPosition.ToReadPositionDto();
        }

        public async Task<ReadPositionDto?> UpdatePositionAsync(int id, UpdatePositionDto updatePositionDto)
        {
            var existingPosition = await _positionRepo.GetPositionByIdAsync(id);
            if (existingPosition == null) 
            {
                throw new KeyNotFoundException($"Position with id {id} not found.");
            }

            ValidateTitle(updatePositionDto.Title);
            await ValidateUniqueTitleAsync(updatePositionDto.Title, id);
            await ValidateJobGradeAsync(updatePositionDto.JobGradeId);
            await ValidateOccupationalLevelAsync(updatePositionDto.OccupationalLevelId);

            var position = updatePositionDto.ToPosition();
            var updatedPosition = await _positionRepo.UpdatePositionAsync(id, position);
            return updatedPosition?.ToReadPositionDto();
        }

        public Task<bool> DeletePositionAsync(int id)
        {
            return _positionRepo.DeletePositionAsync(id);
        }


    }


    
}