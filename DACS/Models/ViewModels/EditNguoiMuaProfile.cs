using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DACS.Models.ViewModels
{
    public class EditNguoiMuaProfile
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
        [Display(Name = "Họ và Tên")]
        public string Ten_NguoiMua { get; set; } // Đổi thành string (không nullable)

        // Bỏ [Required] nếu email liên hệ không bắt buộc
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [Display(Name = "Email liên hệ")]
        public string? Email_NguoiMua { get; set; } // Giữ nullable nếu không bắt buộc

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(10, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải gồm đúng 10 chữ số")]
        public string SDT_NguoiMua { get; set; } // Đổi thành string (không nullable)

        // --- Địa chỉ chi tiết (Giả sử Mã là string) ---
        
        [Display(Name = "Tỉnh/Thành phố")]
        public string Edit_DiaChi_TinhTP { get; set; } // <<< Đổi thành string, thêm Display

        [Display(Name = "Quận/Huyện")]
        public string Edit_DiaChi_QuanHuyen { get; set; } // <<< Đổi thành string, thêm Display

        [Display(Name = "Xã/Phường")]
        public string Edit_DiaChi_XaPhuong { get; set; } // <<< Đổi thành string, thêm Display

       

        [StringLength(200)]
        [Display(Name = "Số nhà, Đường/Ấp/Thôn")]
        public string? Edit_DiaChi_DuongApThon { get; set; } // Giữ string? vì không bắt buộc
        // --- Kết thúc Địa chỉ ---

        [StringLength(10)]
        [Display(Name = "Giới tính")]
        public string? Gender { get; set; }

        [Display(Name = "Ảnh đại diện hiện tại")]
        public string? AvatarUrl { get; set; }

        [Display(Name = "Chọn ảnh đại diện mới (Tối đa 2MB)")]
        // Có thể thêm validation kích thước file ở đây nếu muốn
        // [FileSize(2 * 1024 * 1024)] // Ví dụ (cần tạo FileSizeAttribute)
        // [FileType(new string[] { ".jpg", ".jpeg", ".png", ".gif" })] // Ví dụ (cần tạo FileTypeAttribute)
        public IFormFile? ProfileImageFile { get; set; }

        [Display(Name = "Địa chỉ đầy đủ")]
        public string? HienThiDiaChiDayDu { get; set; }
        

    }
}