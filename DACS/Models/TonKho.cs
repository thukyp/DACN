using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class TonKho
    {
        [Key]
        public int Id { get; set; } // Khóa chính tự tăng

        [Required]
        [Display(Name = "Kho hàng")]
        [StringLength(10)]
        public string MaKho { get; set; } // FK đến KhoHang
        [ForeignKey("MaKho")]
        public virtual KhoHang KhoHang { get; set; }

        [Required]
        [Display(Name = "Sản phẩm")]
        [StringLength(10)]
        public string M_LoaiSP { get; set; } // FK đến SanPham
        [ForeignKey("M_LoaiSP")]
        public virtual LoaiSanPham LoaiSanPham { get; set; }

        [Required]
        [Display(Name = "Khối lượng")]
        public float KhoiLuong { get; set; } // Số lượng tồn


        // Có thể thêm Đơn vị tính nếu cần (FK đến DonViTinh)
        [Required]
        [StringLength(10)]
        public string M_SanPham { get; set; } // PFK
        public string M_DonViTinh { get; set; } 

        [ForeignKey("MaDonViTinh")] 
        public virtual DonViTinh DonViTinh { get; set; }
        [ForeignKey("M_SanPham")]
        public virtual SanPham SanPham { get; set; }

    }
}
