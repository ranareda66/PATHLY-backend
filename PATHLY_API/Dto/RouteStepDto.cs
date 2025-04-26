namespace PATHLY_API.Dto
{
    public class RouteStepDto
    {
        public string Instruction { get; set; }
        public string Distance { get; set; }
        public string Duration { get; set; }
        public double StartLatitude { get; set; }
        public double StartLongitude { get; set; }
        public double EndLatitude { get; set; }
        public double EndLongitude { get; set; }
    }
}
