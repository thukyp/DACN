using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class ChiTietLienHe
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string HoTen { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string NoiDung { get; set; }

        public DateTime NgayGui { get; set; } = DateTime.Now;

        [StringLength(50)] // Tăng độ dài trạng thái
        public string TrangThai { get; set; }
        public string? M_KhachHang { get; set; }
        [ForeignKey("M_KhachHang")]
        public virtual KhachHang KhachHang { get; set; } // FK
    }
}
