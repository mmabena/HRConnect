namespace HRConnect.Api.Models
{
    public class OccupationalLevel
    {
        public int OccupationalLevelId { get; set; }

        public string Description { get; set; } = string.Empty;

        public ICollection<Position> Positions { get; set; } = new List<Position>();
    }
}
