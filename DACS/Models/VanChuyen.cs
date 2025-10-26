using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class VanChuyen // Shipping Info
    {
        [Key]
        [StringLength(10)]
        public string M_VanDon { get; set; }

        [Required]
        [StringLength(100)] // Tăng độ dài
        public string DonViVanChuyen { get; set; }

        // Navigation Property
        public virtual ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

    }
}
