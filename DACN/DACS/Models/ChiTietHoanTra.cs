using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class ChiTietHoanTra
    {
        [Required]
        [StringLength(10)]
        public string M_HoanTra { get; set; } // PFK

        [Required]
        [StringLength(10)]
        // CẢNH BÁO: Liên kết nên đến SP/ChiTietDatHang cụ thể
        public string M_DonHang { get; set; } // PFK

        [Required]
        public int SoLuong { get; set; } // Số lượng trả

        [Required]
        [Column(TypeName = "nvarchar(MAX)")] // Thay Ntext
        public string TrangThaiSanPham { get; set; } // Tình trạng SP trả

        [Required]
        // Nên dùng Decimal
        public long SoTienHoan { get; set; }

        // Navigation Properties
        [ForeignKey("M_HoanTra")]
        public virtual HoanTra HoanTra { get; set; }

        [ForeignKey("M_DonHang")]
        public virtual DonHang DonHang { get; set; } // Liên kết đơn hàng gốc

    }
}
