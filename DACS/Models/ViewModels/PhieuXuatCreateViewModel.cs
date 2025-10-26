using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models.ViewModels
{
    public class PhieuXuatCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn kho xuất.")]
        [Display(Name = "Xuất từ Kho")]
        public string MaKho { get; set; }

        [Required(ErrorMessage = "Ngày xuất là bắt buộc.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày xuất")]
        public DateTime NgayXuat { get; set; } = DateTime.Now;

        [StringLength(500)]
        [Display(Name = "Lý do xuất")]
        public string? LyDoXuat { get; set; }

        // Danh sách các sản phẩm cần xuất
        // Khởi tạo để tránh NullReferenceException trong View nếu không có item nào ban đầu
        public List<ChiTietPhieuXuatItemViewModel> ChiTietItems { get; set; } = new List<ChiTietPhieuXuatItemViewModel>();

        // Dùng để đổ dữ liệu cho các DropdownList trên View
        public IEnumerable<SelectListItem>? KhoHangOptions { get; set; }
        public IEnumerable<SelectListItem>? LoaiSanPhamOptions { get; set; }
        public IEnumerable<SelectListItem>? DonViTinhOptions { get; set; }

        public PhieuXuatCreateViewModel()
        {
            // Thêm một item rỗng ban đầu để form có ít nhất một dòng chi tiết
            // Bạn có thể dùng JavaScript để thêm/xóa dòng chi tiết một cách linh động hơn
            // ChiTietItems.Add(new ChiTietPhieuXuatItemViewModel());
        }
    }
}