using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Sms_Project.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string PlanType { get; set; } = "Free";

        // ==================== NEW PROPERTY ADDED ====================
        [Required]
        public bool IsOnline { get; set; } = false; // Tracks if user is currently logged in
        // ============================================================

        [NotMapped]
        public bool IsPaidPlan => string.Equals(PlanType, "Paid", StringComparison.OrdinalIgnoreCase);

        public int FreeMessagesSent { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}