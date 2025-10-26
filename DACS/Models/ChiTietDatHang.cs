using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class ChiTietDatHang // Order Detail
    {
        [Key]
        public string M_CTDatHang { get; set; }
        
        [StringLength(10)]
        public string? M_SanPham { get; set; } // PFK

        
        [StringLength(10)]
        public string? M_DonHang { get; set; } // PFK

        [Required]
        // Nên dùng Decimal
        public long GiaDatHang { get; set; }

        [Required]
        // Nên dùng Decimal
        public long TongTien { get; set; }

        [Required]
        public float Khoiluong { get; set; }

        
        [StringLength(10)]
        // CẢNH BÁO: Cột này có vẻ thừa, nên bỏ đi.
        public string? M_KhachHang { get; set; } // FK?

        // Navigation Properties
        [ForeignKey("M_SanPham")]
        public virtual SanPham? SanPham { get; set; }

        [ForeignKey("M_DonHang")]
        public virtual DonHang? DonHang { get; set; }

        [ForeignKey("M_KhachHang")]
        public virtual KhachHang? KhachHang { get; set; } // Liên kết này có cần thiết?
        public DateTime NgayTao { get; set; }

        public string TrangThaiDonHang { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        
    }
}
