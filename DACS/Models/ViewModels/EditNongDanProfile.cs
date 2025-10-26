using System.ComponentModel.DataAnnotations;

namespace DACS.Models.ViewModels
{
    public class EditNongDanProfile
    {
        [Required(ErrorMessage = "Vui lòng nhập tên nông dân hoặc tên trang trại.")]
        [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự.")]
        [Display(Name = "Tên Nông Dân / Trang Trại")]
        public string? Ten_NongDan { get; set; } // Sử dụng string?

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [Display(Name = "Email liên hệ")]
        public string? Email_NongDan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [RegularExpression(@"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$", ErrorMessage = "Số điện thoại không hợp lệ.")] // Regex cơ bản cho SĐT
        [Display(Name = "Số điện thoại")]
        public string? SDT_NongDan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ.")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự.")]
        [Display(Name = "Địa chỉ Nông Trại / Liên Hệ")]
        public string? DiaChi_NongDan { get; set; }

    }
}
