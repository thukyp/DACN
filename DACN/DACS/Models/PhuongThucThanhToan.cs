using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class PhuongThucThanhToan // Payment Method
    {
        [Key]
        [StringLength(10)]
        public string M_PhuongThuc { get; set; }

        [Required]
        [StringLength(100)]
        public string TenPhuongThuc { get; set; }

        // Navigation Property
        public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    }
}
