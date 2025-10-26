// File: Models/ViewModels/CreatePhieuXuatViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models.ViewModels // Đảm bảo đúng namespace
{
    public class CreatePhieuXuatViewModel
    {
        [DataType(DataType.DateTime)]
        [Display(Name = "Ngày xuất")]
        public DateTime? NgayXuat { get; set; } = DateTime.Today; // Gán giá trị mặc định là ngày hiện tại

        [Required(ErrorMessage = "Vui lòng chọn kho xuất.")]
        [Display(Name = "Kho xuất")]
        public string? MaKho { get; set; } // Sẽ lấy từ dropdown

        [StringLength(255)]
        [Display(Name = "Lý do xuất")]
        public string? LyDoXuat { get; set; }

        // Danh sách các chi tiết sản phẩm muốn xuất
        // Phần này sẽ phức tạp hơn trên giao diện (thường dùng JS để thêm/xóa dòng)
        public List<ChiTietPhieuXuatInputModel> ChiTietItems { get; set; } = new List<ChiTietPhieuXuatInputModel>();

        // --- Dữ liệu cho các Dropdown trên Form ---
        public List<SelectListItem>? KhoHangOptions { get; set; }
        public List<SelectListItem>? SanPhamOptions { get; set; }
        public List<SelectListItem>? DonViTinhOptions { get; set; } // Có thể load động dựa trên sản phẩm

        // Có thể thêm các trường khác nếu cần
    }

    // Lớp phụ trợ để đại diện cho một dòng nhập chi tiết trên form
    public class ChiTietPhieuXuatInputModel
    {
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm.")]
        [Display(Name = "Sản phẩm")]
        public string? MaSanPham { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn đơn vị tính.")]
        [Display(Name = "ĐVT")]
        public string? MaDonViTinh { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
        [Range(1, long.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        [Display(Name = "Số lượng")]
        public long SoLuong { get; set; }

        // Thêm đơn giá hoặc các trường khác nếu cần
        // [Display(Name = "Đơn giá")]
        // public decimal DonGia { get; set; }
    }
}