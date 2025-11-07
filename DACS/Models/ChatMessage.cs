using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class ChatMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; }  // Ai gửi (user hoặc admin)

        [Required]
        public string ReceiverId { get; set; } // Gửi cho ai

        public string SenderName { get; set; }

        [StringLength(500)]
        public string? Message { get; set; }

        public DateTime SentTime { get; set; } = DateTime.Now;
        public string? M_KhachHang { get; set; }

        [ForeignKey("M_KhachHang")]
        public virtual KhachHang KhachHang { get; set; }
        public bool IsFromAdmin { get; set; }
        [StringLength(255)]
        public string? ImageUrl { get; set; }
    }
}
