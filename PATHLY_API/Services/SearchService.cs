using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;
using PATHLY_API.Models;
using System;

public class SearchService
{
    private readonly ApplicationDbContext _context;

    public SearchService(ApplicationDbContext context) => _context = context;

    public void AddSearch(decimal latitude, decimal longitude)
    {
        var newsearch = new Search
        {
            Latitude = latitude,
            Longitude = longitude
        };

        _context.Searchs.Add(newsearch);
        _context.SaveChanges(); 
    }

    public void RemoveSearch(int searchId)
    {
        var search = _context.Searchs.FirstOrDefault(r => r.Id == searchId);

        if (search is null)
            throw new KeyNotFoundException($"Report with ID {searchId} not found.");

        _context.Searchs.Remove(search);
        _context.SaveChanges();
    }

    public async Task<List<Search>> GetAllSearchAsync() => await _context.Searchs.ToListAsync();
}
