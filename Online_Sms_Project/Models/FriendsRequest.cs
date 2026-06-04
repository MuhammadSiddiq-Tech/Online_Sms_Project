using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Sms_Project.Models
{
    public class FriendRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SenderId { get; set; }

        [ForeignKey("SenderId")]
        public User Sender { get; set; } = null!;

        [Required]
        public int ReceiverId { get; set; }

        [ForeignKey("ReceiverId")]
        public User Receiver { get; set; } = null!;

        [Required]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}