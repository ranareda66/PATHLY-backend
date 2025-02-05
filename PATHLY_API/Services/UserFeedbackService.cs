using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PATHLY_API.Data;
using PATHLY_API.Models;

public class UserFeedbackService
{
    private readonly ApplicationDbContext _context;

    public UserFeedbackService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Submit Feedback (INSERT into DB)
    public void SubmitFeedback(int tripId, int userId, int tripRating, string tripComment, int alertRating, string alertComment)
    {
        var feedback = new UserFeedback
        {
            TripId = tripId,
            UserId = userId,
            TripRating = tripRating,
            TripComment = tripComment,
            AlertRating = alertRating,
            AlertComment = alertComment
        };

        _context.UserFeedbacks.Add(feedback);
        _context.SaveChanges(); 
    }

    //Delete Feedback (DELETE from DB)
    public bool DeleteFeedback(int feedbackId, int userId)
    {
        var feedback = _context.UserFeedbacks.FirstOrDefault(f => f.FeedbackId == feedbackId && f.UserId == userId);
        if (feedback != null)
        {
            _context.UserFeedbacks.Remove(feedback);
            _context.SaveChanges(); 
            return true;
        }
        return false;
    }

    //Update Feedback (UPDATE in DB)
    public bool UpdateFeedback(int feedbackId, int tripRating, string tripComment, int alertRating, string alertComment)
    {
        var feedback = _context.UserFeedbacks.Find(feedbackId);
        if (feedback != null)
        {
            feedback.TripRating = tripRating;
            feedback.TripComment = tripComment;
            feedback.AlertRating = alertRating;
            feedback.AlertComment = alertComment;

            _context.SaveChanges(); 
            return true;
        }
        return false;
    }
}
