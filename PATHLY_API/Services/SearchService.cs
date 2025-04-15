using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;
using PATHLY_API.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

public class SearchService
{
    private readonly ApplicationDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly string _googleMapsApiKey;

    public SearchService(ApplicationDbContext context, HttpClient httpClient, IConfiguration config)
    {
        _context = context;
        _httpClient = httpClient;
        _googleMapsApiKey = config["GoogleMaps:ApiKey"];
    }


    public async Task<GeocodingResult> SearchForAddress(string address , ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!int.TryParse(userIdClaim, out int userId))
            throw new UnauthorizedAccessException($"Invalid User ID format. Value received: '{userIdClaim}'");

        var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={_googleMapsApiKey}";
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var data = JsonDocument.Parse(json);

        var result = data.RootElement.GetProperty("results")[0];
        var formattedAddress = result.GetProperty("formatted_address").GetString();
        var location = result.GetProperty("geometry").GetProperty("location");

        var latitude = location.GetProperty("lat").GetDouble();
        var longitude = location.GetProperty("lng").GetDouble();

        var newSearch = new Search
        {
            Latitude = (decimal)latitude,
            Longitude = (decimal)longitude,
            FormattedAddress = formattedAddress,  
            UserId = userId  
        };

        _context.Searchs.Add(newSearch);
        await _context.SaveChangesAsync();

        return new GeocodingResult
        {
            FormattedAddress = formattedAddress,
            Latitude = latitude,
            Longitude = longitude
        };
    }


    public void RemoveSearch(int searchId)
    {
        var search = _context.Searchs.FirstOrDefault(r => r.Id == searchId);

        if (search is null)
            throw new KeyNotFoundException($"Report with ID {searchId} not found.");

        _context.Searchs.Remove(search);
        _context.SaveChanges();
    }

    public async Task<List<Search>> GetAllSearchAsync()
    {
        return await _context.Searchs
            .Select(s => new Search
            {
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                FormattedAddress = s.FormattedAddress,  
            })
            .ToListAsync();
    }
}
