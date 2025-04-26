using PATHLY_API.Models.Enums;

public class TripDto
{
    public int Id { get; set; }
    public double StartLatitude { get; set; }
    public double StartLongitude { get; set; }
    public double EndLatitude { get; set; }
    public double EndLongitude { get; set; }
    public double Distance { get; set; }
    public string RoutePolyline { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; }
    public bool UsedRoadPrediction { get; set; }
}