using PATHLY_API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PATHLY_API.Data;

public class RoadService
{
    private readonly ApplicationDbContext _context;
    // Inject the DbContext into the service
    public RoadService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Method to update the road condition
    public async Task UpdateConditionAsync(int roadId, string newCondition)
    {
        var road = await _context.Roads.FindAsync(roadId);
        if (road != null)
        {
            road.Conditions = newCondition;
            road.LastUpdate = DateTime.Now; // Optionally update the LastUpdate field
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new ArgumentException("Road not found.");
        }
    }


    // Method to update the LastUpdate field
    public async Task UpdateLastUpdatedAsync(int roadId)
    {
        var road = await _context.Roads.FindAsync(roadId);
        if (road != null)
        {
            road.LastUpdate = DateTime.Now;
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new ArgumentException("Road not found.");
        }
    }
}