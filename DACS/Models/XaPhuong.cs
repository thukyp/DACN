using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace DACS.Models // <-- Kiểm tra lại namespace
{
    public class XaPhuong
    {
        [Key] // Khóa chính
        [StringLength(10)] // Giới hạn độ dài Mã Xã/Phường
        [Display(Name = "Mã Xã/Phường")]
        public string MaXa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Tên Xã/Phường.")]
        [StringLength(100)]
        [Display(Name = "DiaChi_XaPhuong")]
        public string TenXa { get; set; }

        // Thêm các cột khác nếu cần (ví dụ: Loại Đơn vị - Xã / Phường / Thị trấn)
        // [StringLength(50)]
        // public string? LoaiHinh { get; set; }

        // --- Khóa ngoại đến QuanHuyen ---
        
        [StringLength(10)] // Phải khớp kiểu và độ dài với QuanHuyen.MaQuan
        [Display(Name = "Quận/Huyện")]
        public string MaQuan { get; set; } // Foreign Key property

        [ForeignKey("MaQuan")] // Chỉ định FK cho navigation property dưới
        public virtual QuanHuyen QuanHuyen { get; set; } // Navigation property đến Quận/Huyện
                                                         // --- Kết thúc FK ---

        // Navigation Property: Một xã có nhiều khách hàng (nếu cần)
        // public virtual ICollection<KhachHang> KhachHangs { get; set; } = new List<KhachHang>();
    }
}