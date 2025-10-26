using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // Cho ICollection

namespace DACS.Models // <-- Kiểm tra lại namespace
{
    public class TinhThanhPho
    {
        [Key] // Khóa chính
        [StringLength(10)] // Giới hạn độ dài Mã Tỉnh/TP
        public string MaTinh { get; set; }

        
        [StringLength(100)]
        [Display(Name = "Tên Tỉnh/Thành phố")]
        public string TenTinh { get; set; }
        public virtual ICollection<QuanHuyen> QuanHuyens { get; set; } = new List<QuanHuyen>();

        // Navigation Property: Một tỉnh có nhiều khách hàng (nếu cần)
        // public virtual ICollection<KhachHang> KhachHangs { get; set; } = new List<KhachHang>();
    }
}