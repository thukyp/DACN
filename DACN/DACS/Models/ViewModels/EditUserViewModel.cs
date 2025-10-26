// File: Models/ViewModels/EditUserViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering; // Cần cho SelectListItem
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models.ViewModels // Đảm bảo đúng namespace của bạn
{
    public class EditUserViewModel
    {
        // --- Thông tin định danh người dùng (Thường không cho sửa) ---
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = string.Empty; // Hiển thị, không nên cho sửa trực tiếp

        // --- Thông tin hồ sơ có thể sửa ---
        [Required(ErrorMessage = "Vui lòng nhập Họ tên.")]
        [Display(Name = "Họ và Tên")]
        [StringLength(100)]
        public string? FullName { get; set; } // Giả sử ApplicationUser có trường này

        // Lưu ý: Việc sửa Email có thể cần quy trình xác nhận lại email
        [Required(ErrorMessage = "Vui lòng nhập Email.")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        // --- Quản lý Vai trò ---
        // Danh sách TẤT CẢ các vai trò có trong hệ thống để hiển thị (ví dụ: dạng Checkbox list)
        public List<SelectListItem> AllRolesOptions { get; set; } = new List<SelectListItem>();

        // Danh sách tên các vai trò MÀ NGƯỜI DÙNG NÀY ĐANG CÓ (để check vào checkbox tương ứng)
        // Hoặc bạn có thể dùng danh sách này để hiển thị riêng các vai trò hiện tại
        public List<string> CurrentUserRoles { get; set; } = new List<string>();

        // Danh sách tên các vai trò ĐƯỢC CHỌN từ form gửi lên (khi POST)
        // Thuộc tính này sẽ nhận giá trị từ các checkbox hoặc multi-select list trên form
        [Display(Name = "Vai trò được gán")]
        public List<string> SelectedRoles { get; set; } = new List<string>();


        // --- Thông tin trạng thái (Thường để hiển thị) ---
        [Display(Name = "Email đã xác nhận?")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Tài khoản bị khóa?")]
        public bool IsLocked { get; set; }

        [Display(Name = "Khóa đến")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTimeOffset? LockoutEnd { get; set; }

        // --- Constructor (Tùy chọn, có thể hữu ích) ---
        public EditUserViewModel() { }

        // Constructor để khởi tạo từ ApplicationUser (Ví dụ)
        public EditUserViewModel(ApplicationUser user)
        {
            UserId = user.Id;
            UserName = user.UserName ?? "";
            FullName = user.FullName; // Lấy FullName nếu model ApplicationUser có
            Email = user.Email ?? "";
            PhoneNumber = user.PhoneNumber;
            EmailConfirmed = user.EmailConfirmed;
            IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;
            LockoutEnd = user.LockoutEnd;
            // CurrentUserRoles và AllRolesOptions sẽ được nạp từ Controller
        }
    }
}