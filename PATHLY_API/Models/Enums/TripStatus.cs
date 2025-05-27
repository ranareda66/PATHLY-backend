using System.Text.Json.Serialization;

namespace PATHLY_API.Models.Enums
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TripStatus
    {
        InProgress,
        Completed ,
        Cancelled
    }
} 

