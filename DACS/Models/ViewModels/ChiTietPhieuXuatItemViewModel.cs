using System.ComponentModel.DataAnnotations;

namespace DACS.Models.ViewModels
{
    public class ChiTietPhieuXuatItemViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm.")]
        [Display(Name = "Sản phẩm")]
        public string M_LoaiSP { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn đơn vị tính.")]
        [Display(Name = "ĐVT")]
        public string M_DonViTinh { get; set; } // Sẽ khớp với đơn vị tính của sản phẩm và tồn kho

        [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        [Display(Name = "Số lượng xuất")]
        public long SoLuong { get; set; }

        // Các thuộc tính khác để hiển thị trên form nếu cần (ví dụ: Tên sản phẩm, Tên ĐVT)
        public string? TenSanPham { get; set; }
        public string? TenDonViTinh { get; set; }
        public long TonKhoHienTai { get; set; } // Để hiển thị tồn kho khi chọn sản phẩm (nâng cao)
    }
}