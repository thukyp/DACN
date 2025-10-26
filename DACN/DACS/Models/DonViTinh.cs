using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class DonViTinh // Unit of Measure
    {
        [Key]
        [StringLength(10)]
        public string M_DonViTinh { get; set; }

        [Required]
        [StringLength(100)]
        public string TenLoaiTinh { get; set; }

        // Navigation Properties
        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
        public virtual ICollection<ChiTietThuGom> ChiTietThuGoms { get; set; } = new List<ChiTietThuGom>();
    }

}
