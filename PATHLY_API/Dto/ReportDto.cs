using PATHLY_API.Models;
using PATHLY_API.Models.Enums;

namespace PATHLY_API.Dto
{
    public class ReportDto
    {
        public int Id { get; set; }
        public string ReportType { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ReportStatus? Status { get; set; }

        public ICollection<Image> Attachments = new List<Image>();

    }
}