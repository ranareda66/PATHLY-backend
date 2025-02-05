using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PATHLY_API.Models
{

    public class UserFeedback
    {
        [Key]
        public int FeedbackId { get; set; }

        [Required]
        public int TripId { get; set; } // Foreign key reference to Trip

        [Required]
        public int UserId { get; set; } // Foreign key reference to User

        [Required]
        public int TripRating { get; set; } // Rating for the trip

        public string TripComment { get; set; } = string.Empty; // Optional comment

        public int AlertRating { get; set; } // Rating for alerts

        public string AlertComment { get; set; } = string.Empty; // Optional alert comment

        public DateTime SubmittedAt { get; set; } = DateTime.Now; // Auto-set on creation

        // Navigation properties
        [ForeignKey("UserId")]
        public User? User { get; set; }

       // [ForeignKey("TripId")]
        // public Trip? Trip { get; set; }
    } 
}


