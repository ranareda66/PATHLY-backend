using PATHLY_API.Data;
using PATHLY_API.Models;
using System;
using System.Linq;

public class RoadRecommendationService
{
    private readonly ApplicationDbContext _context;

    public RoadRecommendationService(ApplicationDbContext context)
    {
        _context = context;
    }

    //Inserts a new recommendation
    public void GenerateRecommendation(int userId, int roadId, decimal rqiScore, bool shortestPath, bool mostUsed)
    {
        var recommendation = new RoadRecommendation
        {
            UserId = userId,
            RoadId = roadId,
            RQIScore = rqiScore,
            ShortestPath = shortestPath,
            MostUsed = mostUsed,
            Recommended = false
        };

        _context.RoadRecommendations.Add(recommendation);
        _context.SaveChanges(); 
    }

    //Updates recommendation status
    public void UpdateRecommendationStatus(int recommendationId, bool status)
    {
        var recommendation = _context.RoadRecommendations.Find(recommendationId);
        if (recommendation != null)
        {
            recommendation.Recommended = status;
            _context.SaveChanges();
        }
    }

}

