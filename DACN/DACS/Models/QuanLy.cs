using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class QuanLy
    {
        [Key] // Giả sử đây là PK
        [Required]
        [StringLength(10)]
        public string M_QuanLy { get; set; } // Varchar(10) PK & FK to QuanLy

        [Column(TypeName = "nvarchar(MAX)")] // Thay Ntext
        public string? YeuCau { get; set; }

        // Cột M_QuanLy thứ 2 trong SQL bị trùng tên, cần sửa trong SQL. Bỏ qua ở đây.

        [Required]
        [StringLength(10)]
        public string M_HoanTra { get; set; } // Varchar(10) FK

        [Required]
        [StringLength(10)]
        public string M_DonHang { get; set; } // Varchar(10) FK

        // Navigation Properties
        [ForeignKey("M_QuanLy")]
        public virtual Owner Owner { get; set; }

        [ForeignKey("M_HoanTra")]
        public virtual HoanTra HoanTra { get; set; }

        [ForeignKey("M_DonHang")]
        public virtual DonHang DonHang { get; set; }

        [Required]
        [StringLength(450)] // Khớp với Id của AspNetUsers
        public string UserId { get; set; } // Foreign Key

        public virtual ICollection<ChiTietThuGom> ChiTietThuGoms { get; set; } = new List<ChiTietThuGom>();
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}
