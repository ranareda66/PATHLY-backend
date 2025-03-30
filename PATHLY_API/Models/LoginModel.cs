﻿using System.ComponentModel.DataAnnotations;

namespace PATHLY_API.Models
{ 
    public class LoginModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
