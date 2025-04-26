namespace PATHLY_API.Dto
{
    public class RouteDto
    {
        public string Polyline { get; set; }
        public List<RouteStepDto> Steps { get; set; }
        public double DistanceKm { get; set; }
        public string Duration { get; set; }
    }
}
