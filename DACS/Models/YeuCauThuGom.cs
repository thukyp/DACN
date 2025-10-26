using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class YeuCauThuGom // Collection Request
    {
        [Key]
        [StringLength(10)]
        public string M_YeuCau { get; set; }

        [Required]
        public DateTime NgayYeuCau { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)] // Tăng độ dài
        public string TrangThai { get; set; }

        // --- Missing FK to NongDan based on logic/ERD? ---
        [Required]
        [StringLength(10)]
        public string M_KhachHang { get; set; }
        [ForeignKey("M_KhachHang")]
        public virtual KhachHang KhachHang { get; set; }

        // Navigation Property
        public virtual ICollection<ChiTietThuGom> ChiTietThuGoms { get; set; } = new List<ChiTietThuGom>();
        public DateTime ThoiGianSanSang { get; set; }
        public string? GhiChu { get; set; }
        [Column("Thoi_Gian_HT")]
        public DateTime ThoiGianHoanThanh { get;  set; }
        public string? M_QuanLy { get; set; } // Thuộc tính khóa ngoại

        [ForeignKey("M_QuanLy")] // <<< THÊM DÒNG NÀY
        public virtual ApplicationUser? QuanLy { get; set; } // Thuộc tính navigation
    }
}
