using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DACS.Models
{
    public class LoTonKho
    {
        [Key]
        [StringLength(20)]
        [Display(Name = "Mã Lô")]
        public string MaLoTonKho { get; set; } // Ví dụ: LOT-20251105-001

        [Required]
        [StringLength(10)]
        [Display(Name = "Sản phẩm")]
        public string M_SanPham { get; set; } // Khóa ngoại đến SanPham

        [Required(ErrorMessage = "Vui lòng chọn kho hàng.")]
        [StringLength(10)]
        [Display(Name = "Kho hàng")]
        public string MaKho { get; set; } // Khóa ngoại đến KhoHang

        [Required]
        [Display(Name = "Ngày nhập kho")]
        [DataType(DataType.Date)]
        public DateTime NgayNhapKho { get; set; }

        [Display(Name = "Hạn sử dụng")]
        [DataType(DataType.Date)]
        public DateTime? HanSuDung { get; set; }

        // <<< SỬA LẠI DISPLAY NAME CHO RÕ NGHĨA ---
        [Required]
        [Display(Name = "Khối lượng ban đầu")] // <<< Sửa
        public decimal KhoiLuongBanDau { get; set; }

        [Required]
        [Display(Name = "Khối lượng còn lại")] // <<< Sửa
        public decimal KhoiLuongConLai { get; set; }
        // --- KẾT THÚC SỬA ---

        [Required]
        [StringLength(10)]
        [Display(Name = "Đơn vị tính")]
        public string M_DonViTinh { get; set; } // Khóa ngoại đến DonViTinh

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        // --- Navigation Properties ---

        [ForeignKey("M_SanPham")]
        public virtual SanPham SanPham { get; set; }

        [ForeignKey("M_DonViTinh")]
        public virtual DonViTinh DonViTinh { get; set; }

        [ForeignKey("MaKho")]
        public virtual KhoHang KhoHang { get; set; }

        // MỘT lô này được tạo thành từ NHIỀU chi tiết thu gom
        public virtual ICollection<ChiTietThuGom> ChiTietThuGoms { get; set; }

        public LoTonKho()
        {
            ChiTietThuGoms = new HashSet<ChiTietThuGom>();
        }
    }
}