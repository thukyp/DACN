using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity; // Cần thêm

namespace DACS.Models
{
    public class SanPhamYeuThich
    {
        [Key]
        [StringLength(10)] 
        public string M_YeuThich { get; set; } // Khóa chính (cần logic tạo mã)

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(10)]
        public string M_SanPham { get; set; } // FK

        [Required]
        public DateTime NgayThem { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [ForeignKey("M_SanPham")]
        public virtual SanPham? SanPham { get; set; }
    }
}