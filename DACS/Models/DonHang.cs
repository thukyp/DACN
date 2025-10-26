using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DACS.Models
{
    public class DonHang // Order
    {
     
        [Key]
        [StringLength(10)]
        public string? M_DonHang { get; set; }

        
        public DateTime NgayDat { get; set; } = DateTime.UtcNow;

        // Nên cho phép NULL ban đầu
        public DateTime? NgayGiao { get; set; } // DateTime?

        
        [StringLength(50)] // Tăng độ dài trạng thái
        public string TrangThai { get; set; }

        public string TrangThaiThanhToan { get; set; }
        public string? LyDoHoanTra { get; set; } // Lý do khách hàng hoàn trả
        public DateTime? NgayHoanTra { get; set; }
        public string? TrangThaiHoanTra { get; set; }

        public bool DaTruTonKho { get; set; } = false;
        [StringLength(10)]
        public string M_VanDon { get; set; } // FK

        
        [StringLength(10)]
        public string M_PhuongThuc { get; set; } // FK

        // Navigation Properties
        [ForeignKey("M_VanDon")]
        public virtual VanChuyen VanChuyen { get; set; }

        [ForeignKey("M_PhuongThuc")]
        public virtual PhuongThucThanhToan PhuongThucThanhToan { get; set; }

        public virtual ICollection<ChiTietDatHang> ChiTietDatHangs { get; set; } = new List<ChiTietDatHang>();
        public virtual ICollection<ChiTietHoanTra> ChiTietHoanTras { get; set; } = new List<ChiTietHoanTra>();
        public virtual ICollection<QuanLy> QuanLyNhaps { get; set; } = new List<QuanLy>();
        public virtual ICollection<ChiTietThuGom> ChiTietThuGoms { get; set; } = new List<ChiTietThuGom>();
        public virtual ICollection<YeuCauThuGom> YeuCauThuGoms { get; set; } = new List<YeuCauThuGom>();
        public string ShippingAddress { get; set; }
        
        [StringLength(10)]
        
        public string? M_KhachHang { get; set; }
        [ForeignKey("M_KhachHang")]
        public virtual KhachHang KhachHang { get; set; } // FK
        public float TotalPrice { get; internal set; }
         // Giữ lại nếu cột DB là NOT NULL, bỏ đi nếu cột DB cho phép NULL
        public string Notes { get; set; } // Thêm thuộc tính Notes khớp với cột DB
        [Required]
        [StringLength(100)]
        public string Tendathang { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone]
        [StringLength(10)]
        public string SoDienThoaidathang { get; set; }

       
        // ---> KẾT THÚC THÊM <---
    }
}
