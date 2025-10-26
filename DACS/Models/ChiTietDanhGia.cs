using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class ChiTietDanhGia // Review Detail
    {
        [Required]
        [StringLength(10)]
        public string? M_KhachHang { get; set; } // PFK

        [Required]
        [StringLength(10)]
        public string? M_SanPham { get; set; } // PFK

        // Nên là int 1-5
        [StringLength(10)]
        public string? MucDoHaiLong { get; set; } // Varchar(10) NULL

        [Column(TypeName = "nvarchar(MAX)")] // Thay Ntext
        public string? MoTa_DanhGia { get; set; } // Ntext NULL

        public DateTime NgayDanhGia { get; set; } = DateTime.UtcNow; // Thêm ngày đánh giá

        // Navigation Properties
        [ForeignKey("M_KhachHang")]
        public virtual KhachHang? KhachHang { get; set; }

        [ForeignKey("M_SanPham")]
        public virtual SanPham? SanPham { get; set; }

    }
}
