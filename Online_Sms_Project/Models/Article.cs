using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Sms_Project.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

        // Tracks which user wrote the article
        [Required]
        public int AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public virtual User? Author { get; set; }
    }
}