using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class GiamGia // Discount
    {
        [Key]
        [StringLength(10)]
        public string M_GiamGia { get; set; }

        [Required]
        [StringLength(100)]
        public string TenGiamGia { get; set; }

        [Required]
        public int SoLuong { get; set; } // Giới hạn sử dụng?

        [Required]
        // Nên dùng Decimal và làm rõ ý nghĩa
        public long Tien { get; set; }

        // ----- Các thuộc tính đã thêm vào ở lần trước -----
        [Required]
        [StringLength(50)]
        public string LoaiGiamGia { get; set; } // "Percentage", "FixedAmount"

        public string? MoTa { get; set; }

        [Required]
        public DateTime NgayBatDau { get; set; }

        public DateTime? NgayKetThuc { get; set; }

        [Required]
        public bool IsActive { get; set; }
       

        [Column(TypeName = "decimal(18, 2)")] 
        public decimal? GiaTriDonHangToiThieu { get; set; }

        // Navigation Properties for Many-to-Many
        public virtual ICollection<SanPhamGiamGia> SanPhamGiamGias { get; set; } = new List<SanPhamGiamGia>();
        public virtual ICollection<LoaiSanPhamGiamGia> LoaiSanPhamGiamGias { get; set; } = new List<LoaiSanPhamGiamGia>();

    }
}
