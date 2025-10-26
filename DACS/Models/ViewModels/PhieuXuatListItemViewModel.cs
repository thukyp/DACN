using System;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models.ViewModels
{
    public class PhieuXuatListItemViewModel
    {
        public int MaPhieuXuat { get; set; }

        [Display(Name = "Mã Phiếu")]
        public string MaPhieuXuatHienThi => $"PX{MaPhieuXuat:D5}"; // Định dạng lại mã phiếu

        [Display(Name = "Người Nhận")]
        public string? NguoiNhan { get; set; }

        [Display(Name = "Ngày Xuất")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime NgayXuat { get; set; }

        [Display(Name = "Kho Xuất")]
        public string? TenKhoXuat { get; set; }

        [Display(Name = "Tổng SL")] // Tổng số lượng các mặt hàng
        public long TongSoLuongItems { get; set; }

        [Display(Name = "Tổng Giá Trị")] // Nếu có giá, nếu không có thể là tổng khối lượng
        [DisplayFormat(DataFormatString = "{0:N0} VNĐ")]
        public decimal TongGiaTri { get; set; } // Hoặc kiểu khác cho khối lượng

        [Display(Name = "Trạng Thái")]
        public string? TrangThai { get; set; }
    }
}