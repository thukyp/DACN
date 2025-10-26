// File: Models/ViewModels/UserViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DACS.Models.ViewModels
{
    // Đại diện cho một dòng trong bảng người dùng
    public class UserViewModel
    {
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "Tên người dùng")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Họ tên")]
        public string? FullName { get; set; } // Giả sử ApplicationUser có

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Vai trò")]
        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>(); // Có thể có nhiều vai trò

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Không xác định"; // "Hoạt động", "Bị khóa", "Chờ xác minh"

        [Display(Name = "Ngày tham gia")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTimeOffset CreatedDate { get; set; } // Nên lấy từ cột ngày tạo thực tế của User

        public bool IsLocked { get; set; }
        public bool EmailConfirmed { get; set; } // Cần để xác định trạng thái "Chờ xác minh"
    }

    // Chứa dữ liệu thống kê
    public class UserStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int PendingUsers { get; set; }
        public int LockedUsers { get; set; }
    }

    // ViewModel chính cho trang quản lý người dùng
    public class UserManagementViewModel
    {
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        public UserStatsViewModel Statistics { get; set; } = new UserStatsViewModel();

        // Dùng cho Filter và Search
        [Display(Name = "Tìm kiếm")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Vai trò")]
        public string? RoleFilter { get; set; } // Lưu tên vai trò được chọn

        [Display(Name = "Trạng thái")]
        public string? StatusFilter { get; set; } // "active", "locked", "pending"

        public List<SelectListItem> RoleOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> StatusOptions { get; set; } = new List<SelectListItem>();

        // Dùng cho Pagination
        // Dùng cho Pagination
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; } // Thêm PageSize nếu bạn muốn hiển thị nó hoặc dùng ở View

        // *** THÊM THUỘC TÍNH NÀY ***
        public int TotalItems { get; set; } // Tổng số lượng kết quả khớp với bộ lọc

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }

    // ViewModel cho Form thêm User (Cần thêm mật khẩu và xác nhận mật khẩu)
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Họ tên.")]
        [Display(Name = "Họ và Tên")]
        public string? FullName { get; set; } // Cho phép null nếu ApplicationUser không có

        [Required(ErrorMessage = "Vui lòng nhập Tên đăng nhập.")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Email.")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        [Display(Name = "Vai trò")]
        public string Role { get; set; } = string.Empty; // Tên vai trò được chọn

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "{0} phải dài ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 6)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Dùng để hiển thị dropdown vai trò trên form
        public List<SelectListItem>? RoleOptions { get; set; }
    }
}