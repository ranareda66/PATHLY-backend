using System.Text.Json.Serialization;

namespace PATHLY_API.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ProblemType
    {
        Pothole,
        Crack,
        Bump,
        Other
    }
}
