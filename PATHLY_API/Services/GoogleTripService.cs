using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace PATHLY_API.Services
{
    public class GoogleTripService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GoogleTripService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleMaps:ApiKey"];
        }

        public async Task<RouteResult> GetRouteBetweenCoordinatesAsync(
            double startLat, double startLng,
            double endLat, double endLng)
        {
            try
            {
                var url = $"https://maps.googleapis.com/maps/api/directions/json?" +
                         $"origin={startLat.ToString(CultureInfo.InvariantCulture)},{startLng.ToString(CultureInfo.InvariantCulture)}&" +
                         $"destination={endLat.ToString(CultureInfo.InvariantCulture)},{endLng.ToString(CultureInfo.InvariantCulture)}&" +
                         $"key={_apiKey}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                using var responseStream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<GoogleDirectionsResponse>(responseStream);

                if (result?.Routes?.Count == 0)
                    throw new Exception("No route found between the specified coordinates");

                var route = result.Routes[0];
                var polyline = route.OverviewPolyline.Points;
                var distance = route.Legs[0].Distance.Value / 1000.0; // Convert meters to km
                var duration = route.Legs[0].Duration.Text;

                return new RouteResult
                {
                    DistanceKm = distance,
                    Duration = duration,
                    Polyline = polyline,
                    Steps = route.Legs[0].Steps.Select(s => new RouteStep
                    {
                        Instruction = RemoveHtmlTags(s.HtmlInstructions),
                        Distance = s.Distance.Text,
                        Duration = s.Duration.Text,
                        StartLocation = new Coordinates
                        {
                            Latitude = s.StartLocation.Lat,
                            Longitude = s.StartLocation.Lng
                        },
                        EndLocation = new Coordinates
                        {
                            Latitude = s.EndLocation.Lat,
                            Longitude = s.EndLocation.Lng
                        }
                    }).ToList()
                };
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to connect to Google Maps API", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to parse Google Maps response", ex);
            }
        }

        private string RemoveHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html)) return html;
            return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", "");
        }

        #region Response Models
        public class RouteResult
        {
            public double DistanceKm { get; set; }
            public string Duration { get; set; }
            public string Polyline { get; set; }
            public List<RouteStep> Steps { get; set; }
        }

        public class RouteStep
        {
            public string Instruction { get; set; }
            public string Distance { get; set; }
            public string Duration { get; set; }
            public Coordinates StartLocation { get; set; }
            public Coordinates EndLocation { get; set; }
        }

        public class Coordinates
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }

        private class GoogleDirectionsResponse
        {
            [JsonPropertyName("routes")]
            public List<GoogleRoute> Routes { get; set; }
        }

        private class GoogleRoute
        {
            [JsonPropertyName("overview_polyline")]
            public OverviewPolyline OverviewPolyline { get; set; }
            [JsonPropertyName("legs")]
            public List<Leg> Legs { get; set; }
        }

        private class OverviewPolyline
        {
            [JsonPropertyName("points")]
            public string Points { get; set; }
        }

        private class Leg
        {
            [JsonPropertyName("distance")]
            public TextValue Distance { get; set; }
            [JsonPropertyName("duration")]
            public TextValue Duration { get; set; }
            [JsonPropertyName("steps")]
            public List<Step> Steps { get; set; }
        }

        private class Step
        {
            [JsonPropertyName("html_instructions")]
            public string HtmlInstructions { get; set; }
            [JsonPropertyName("distance")]
            public TextValue Distance { get; set; }
            [JsonPropertyName("duration")]
            public TextValue Duration { get; set; }
            [JsonPropertyName("start_location")]
            public Location StartLocation { get; set; }
            [JsonPropertyName("end_location")]
            public Location EndLocation { get; set; }
        }

        private class Location
        {
            [JsonPropertyName("lat")]
            public double Lat { get; set; }
            [JsonPropertyName("lng")]
            public double Lng { get; set; }
        }

        private class TextValue
        {
            [JsonPropertyName("text")]
            public string Text { get; set; }
            [JsonPropertyName("value")]
            public double Value { get; set; }
        }
        #endregion
    }
}