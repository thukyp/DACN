using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class HoanTra
    {
        [Key]
        [StringLength(10)]
        public string M_HoanTra { get; set; }

        [Required]
        public DateTime NgayYeuCau { get; set; }

        [Required]
        [StringLength(50)] // Tăng độ dài
        public string TrangThai { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(MAX)")] // Thay Ntext
        public string LyDo { get; set; }

        [Required]
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<ChiTietHoanTra> ChiTietHoanTras { get; set; } = new List<ChiTietHoanTra>();
        public virtual QuanLy QuanLyNhap { get; set; }

    }
}
