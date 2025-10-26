// File: Models/PhieuXuat.cs (Thêm các thuộc tính sau)
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DACS.Models
{
    public class PhieuXuat
    {
        [Key]
        public int MaPhieuXuat { get; set; } // Hoặc kiểu string nếu mã phức tạp

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime NgayXuat { get; set; }

        [StringLength(200)] // Thêm độ dài nếu muốn
        public string? LyDoXuat { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn kho xuất.")]
        [StringLength(10)]
        [Display(Name = "Kho xuất")]
        public string MaKho { get; set; } // FK đến KhoHang
        [ForeignKey("MaKho")]
        public virtual KhoHang? KhoHang { get; set; }

        // --- THUỘC TÍNH MỚI ĐỀ XUẤT ---
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string? TrangThai { get; set; } // Ví dụ: "Mới tạo", "Đang xử lý", "Đã xuất", "Đã hủy"

        [StringLength(150)]
        [Display(Name = "Người nhận")] // Hoặc tên công ty, địa điểm nhận...
        public string? NguoiNhan { get; set; }
        // --- KẾT THÚC THUỘC TÍNH MỚI ---

        public virtual ICollection<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; } = new List<ChiTietPhieuXuat>();
    }

    // Bạn có thể tạo một enum hoặc class hằng số cho Trạng Thái Phiếu Xuất
    public static class PhieuXuatTrangThai
    {
        public const string MoiTao = "Mới tạo";
        public const string DangXuLy = "Đang xử lý";
        public const string DaXuat = "Đã xuất"; // Hoặc "Đã giao"
        public const string DaHuy = "Đã hủy";
    }
}