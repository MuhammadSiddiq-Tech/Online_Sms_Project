using System;
using System.ComponentModel.DataAnnotations;

namespace Online_Sms_Project.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; } = false; // Notification badge ke liye

        public User Sender { get; set; }
        public User Receiver { get; set; }
    }
}