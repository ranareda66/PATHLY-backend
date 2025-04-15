using System.Text.Json;

namespace PATHLY_API.Services
{
    public class GoogleMapsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GoogleMapsService(HttpClient httpClient , IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleMaps:ApiKey"] ?? throw new ArgumentNullException("Google Maps API key is missing");

        }

        // Get (latitude,longitude) From Any Address ✅
        public async Task<string> GetAddressFromCoordinates(decimal latitude, decimal longitude)
        {
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={latitude},{longitude}&key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            var json = JsonDocument.Parse(content);
            var results = json.RootElement.GetProperty("results");

            if (results.GetArrayLength() > 0)
                return results[0].GetProperty("formatted_address").GetString();

            return "Address not found";
        }

        // Get Distance Between Any 2 Places ✅
        public async Task<double> GetDistanceBetweenPlacesByName(string origin, string destination)
        {
            var url = $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={origin}&destinations={destination}&key={_apiKey}";

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            var data = JsonDocument.Parse(json);
            var root = data.RootElement;

            if (root.TryGetProperty("rows", out var rows) && rows.GetArrayLength() > 0)
            {
                var elements = rows[0].GetProperty("elements");
                if (elements.GetArrayLength() > 0)
                {
                    var element = elements[0];
                    if (element.GetProperty("status").GetString() == "OK")
                    {
                        var distanceInMeters = element.GetProperty("distance").GetProperty("value").GetDouble();
                        return distanceInMeters / 1000.0;
                    }
                }
            }
            return -1;
        }

        public async Task<string> GetTrafficDataAsync(string placeName)
        {
            // First geocode the place name to get coordinates
            var geocodeUrl = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(placeName)}&key={_apiKey}";

            try
            {
                // Step 1: Get coordinates from place name
                var geocodeResponse = await _httpClient.GetAsync(geocodeUrl);
                geocodeResponse.EnsureSuccessStatusCode();
                var geocodeContent = await geocodeResponse.Content.ReadFromJsonAsync<GeocodeResponse>();

                if (geocodeContent?.Results == null || geocodeContent.Results.Length == 0)
                    throw new Exception("Location not found");

                var location = geocodeContent.Results[0].Geometry.Location;

                // Step 2: Get traffic data using coordinates
                var directionsUrl = $"https://maps.googleapis.com/maps/api/directions/json?" +
                                    $"origin={location.Lat},{location.Lng}&" +
                                    $"destination={location.Lat + 0.01},{location.Lng + 0.01}&" +
                                    $"departure_time=now&" +
                                    $"traffic_model=best_guess&" +
                                    $"key={_apiKey}";

                var directionsResponse = await _httpClient.GetAsync(directionsUrl);
                directionsResponse.EnsureSuccessStatusCode();
                return await directionsResponse.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                throw new HttpRequestException($"Google Maps API request failed. URL: {geocodeUrl.Replace(_apiKey, "REDACTED")}", ex);
            }
        }
        
        
        // Helper classes for geocoding response
        private class GeocodeResponse
        {
            public GeocodeResult[] Results { get; set; }
        }

        private class GeocodeResult
        {
            public Geometry Geometry { get; set; }
        }

        private class Geometry
        {
            public Location Location { get; set; }
        }

        private class Location
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }

        //        public async Task<object> GetTrafficDataAsync(double latitude, double longitude)
        //        {
        //            // Create a small route near the clicked point
        //            var url = $"https://maps.googleapis.com/maps/api/directions/json?" +
        //                     $"origin={latitude - 0.005},{longitude - 0.005}&" +  // ~500m SW
        //                     $"destination={latitude + 0.005},{longitude + 0.005}&" +  // ~500m NE
        //                     $"departure_time=now&" +
        //                     $"traffic_model=best_guess&" +
        //                     $"key={_apiKey}";

        //            try
        //            {
        //                var response = await _httpClient.GetAsync(url);
        //                response.EnsureSuccessStatusCode();
        //                var jsonString = await response.Content.ReadAsStringAsync();

        //                // Parse the response to extract relevant traffic data
        //                var jsonDoc = JsonDocument.Parse(jsonString);
        //                var root = jsonDoc.RootElement;

        //                if (root.GetProperty("status").GetString() != "OK")
        //                    throw new Exception("No traffic data available for this location");

        //                var route = root.GetProperty("routes")[0];
        //                var leg = route.GetProperty("legs")[0];

        //                var trafficData = new
        //                {
        //                    Origin = leg.GetProperty("start_address").GetString(),
        //                    Destination = leg.GetProperty("end_address").GetString(),
        //                    NormalDuration = leg.GetProperty("duration").GetProperty("text").GetString(),
        //                    TrafficDuration = leg.GetProperty("duration_in_traffic").GetProperty("text").GetString(),
        //                    Distance = leg.GetProperty("distance").GetProperty("text").GetString(),
        //                    TrafficRatio = CalculateTrafficRatio(
        //                        leg.GetProperty("duration").GetProperty("value").GetInt32(),
        //                        leg.GetProperty("duration_in_traffic").GetProperty("value").GetInt32()),
        //                    Steps = ExtractSteps(leg.GetProperty("steps")),
        //                    OverviewPolyline = route.GetProperty("overview_polyline").GetProperty("points").GetString()
        //                };

        //                return trafficData;
        //            }
        //            catch (HttpRequestException ex)
        //            {
        //                throw new HttpRequestException($"Google Maps API request failed. URL: {url.Replace(_apiKey, "REDACTED")}", ex);
        //            }
        //        }

        //        private double CalculateTrafficRatio(int normalSeconds, int trafficSeconds)
        //        {
        //            return Math.Round((double)(trafficSeconds - normalSeconds) / normalSeconds * 100, 1);
        //        }

        //        private List<object> ExtractSteps(JsonElement steps)
        //        {
        //            var result = new List<object>();
        //            foreach (var step in steps.EnumerateArray())
        //            {
        //                result.Add(new
        //                {
        //                    Instruction = step.GetProperty("html_instructions").GetString(),
        //                    Duration = step.GetProperty("duration").GetProperty("text").GetString(),
        //                    Distance = step.GetProperty("distance").GetProperty("text").GetString(),
        //                    Polyline = step.GetProperty("polyline").GetProperty("points").GetString()
        //                });
        //            }
        //            return result;
        //        }

        // _______________________________________________________________


        //public async Task<string> GetTrafficDataAsync(double latitude, double longitude)
        //{
        //    // Using Directions API to get traffic data
        //    var url = $"https://maps.googleapis.com/maps/api/directions/json?" +
        //             $"origin={latitude},{longitude}&" +
        //             $"destination={latitude + 0.01},{longitude + 0.01}&" + // Nearby location to create a route
        //             $"departure_time=now&" + // Required for traffic data
        //             $"traffic_model=best_guess&" +
        //             $"key={_apiKey}";

        //    try
        //    {
        //        var response = await _httpClient.GetAsync(url);
        //        response.EnsureSuccessStatusCode();
        //        return await response.Content.ReadAsStringAsync();
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        throw new HttpRequestException($"Google Maps API request failed. URL: {url.Replace(_apiKey, "REDACTED")}", ex);
        //    }
        //}
    }
}