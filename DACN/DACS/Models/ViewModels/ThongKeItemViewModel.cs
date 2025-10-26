using System.ComponentModel.DataAnnotations;

namespace DACS.Models.ViewModels
{
    public class ThongKeItemViewModel
    {
        [Display(Name = "Sản phẩm")]
        public string TenSanPham { get; set; } = "N/A";

        [Display(Name = "Đơn vị")]
        public string TenDonViTinh { get; set; } = "N/A";

        [Display(Name = "Đã nhập")]
        public long TongNhap { get; set; } // Tổng nhập trong kỳ

        [Display(Name = "Đã xuất")]
        public long TongXuat { get; set; } // Tổng xuất trong kỳ

        [Display(Name = "Tồn hiện tại")]
        public float TonHienTai { get; set; } // Tồn cuối kỳ (lấy từ TonKho)

        [Display(Name = "Chênh lệch")]
        public long ChenhLech => TongNhap - TongXuat; // Chênh lệch Nhập - Xuất trong kỳ

        // Giữ lại mã để join nếu cần
        public string MaSanPham { get; set; }
        public string MaDonViTinh { get; set; }
    }
}