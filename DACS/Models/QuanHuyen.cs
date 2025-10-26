using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace DACS.Models // <-- Kiểm tra lại namespace
{
    public class QuanHuyen
    {
        [Key] // Khóa chính
        [StringLength(10)] // Giới hạn độ dài Mã Quận/Huyện
        [Display(Name = "Mã Quận/Huyện")]
        public string MaQuan { get; set; }

       
        [StringLength(100)]
        [Display(Name = "Tên Quận/Huyện")]
        public string TenQuan { get; set; }

        // Thêm các cột khác nếu cần (ví dụ: Loại Đơn vị - Quận / Huyện / Thị xã / TP thuộc tỉnh)
        // [StringLength(50)]
        // public string? LoaiHinh { get; set; }

        // --- Khóa ngoại đến TinhThanhPho ---
       
        [StringLength(10)] // Phải khớp kiểu và độ dài với TinhThanhPho.MaTinh
        [Display(Name = "Tỉnh/Thành phố")]
        public string MaTinh { get; set; } // Foreign Key property

        [ForeignKey("MaTinh")] // Chỉ định FK cho navigation property dưới
        public virtual TinhThanhPho TinhThanhPho { get; set; } // Navigation property đến Tỉnh/TP
        // --- Kết thúc FK ---

        // Navigation Property: Một quận/huyện có nhiều xã/phường
        public virtual ICollection<XaPhuong> XaPhuongs { get; set; } = new List<XaPhuong>();

        // Navigation Property: Một quận có nhiều khách hàng (nếu cần)
        // public virtual ICollection<KhachHang> KhachHangs { get; set; } = new List<KhachHang>();
    }
}