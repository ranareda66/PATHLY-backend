using PATHLY_API.Data;
using PATHLY_API.Models;
using System;

public class SearchHistoryService
{
    private readonly ApplicationDbContext _context;

    public SearchHistoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public void AddSearchHistory(int userId, string searchQuery, decimal latitude, decimal longitude)
    {
        var searchHistory = new SearchHistory
        {
            UserId = userId,
            SearchQuery = searchQuery,
            Latitude = latitude,
            Longitude = longitude
        };

        _context.SearchHistories.Add(searchHistory);
        _context.SaveChanges(); 
    }
}
