using System.ComponentModel.DataAnnotations;

namespace DACS.Models.ViewModels
{
    public class EditOrderViewModel
    {
        [Required]
        public string M_DonHang { get; set; } // Để xác định đơn hàng, sẽ là hidden field

        // Thông tin gốc - hiển thị, không sửa (lấy từ DonHang gốc)
        public string? Display_M_KhachHang { get; set; }
        public string? Display_Ten_KhachHang { get; set; }
        public DateTime Display_NgayDat { get; set; }
        public decimal Display_TotalPrice { get; set; }
        public string? Display_TrangThai { get; set; } // Hiển thị trạng thái hiện tại của đơn hàng

        // Các trường cho phép chỉnh sửa
        [Display(Name = "Tên Người Nhận")]
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận.")]
        [StringLength(100, ErrorMessage = "Tên người nhận không được vượt quá 100 ký tự.")]
        public string TenNguoiNhan { get; set; }

        [Display(Name = "SĐT Người Nhận")]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại người nhận.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự.")]
        public string SDTNguoiNhan { get; set; }

        [Display(Name = "Địa Chỉ Giao Hàng")]
        [StringLength(500, ErrorMessage = "Địa chỉ giao hàng không được vượt quá 500 ký tự.")]
        public string? ShippingAddress { get; set; } // Cho phép null nếu không bắt buộc

        [Display(Name = "Ghi Chú Đơn Hàng (Owner)")]
        [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự.")]
        public string? Notes { get; set; } // Cho phép null nếu không bắt buộc

    }
}
