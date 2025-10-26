using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DACS.Models
{
    public class ChiTietPhieuXuat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MaPhieuXuat { get; set; } // FK
        [ForeignKey("MaPhieuXuat")]
        public virtual PhieuXuat PhieuXuat { get; set; }

        [Required]
        [StringLength(10)]
        public string M_LoaiSP { get; set; } // FK
        [ForeignKey("M_LoaiSP")]
        public virtual LoaiSanPham LoaiSanPham { get; set; }


        [Required]
        public string M_DonViTinh { get; set; } // FK
        [ForeignKey("M_DonViTinh")] // Sửa lại tên ForeignKey cho đúng
        public virtual DonViTinh DonViTinh { get; set; }

        [Required]
        public long SoLuong { get; set; }

        // Các trường khác nếu cần: Đơn giá,...
    }
}