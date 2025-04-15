using PATHLY_API.Models.Enums;

namespace PATHLY_API.Dto
{
    public class TripDto
    {
        public int Id { get; set; }
        public string StartLocation { get; set; }
        public string EndLocation { get; set; }
        public double Distance { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public TripStatus Status { get; set; }
    }

}