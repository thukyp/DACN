using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class LoaiSanPhamGiamGia // CategoryDiscount (Junction Table)
    {
        // Composite Primary Key (configured in DbContext using Fluent API)
        [Required]
        [StringLength(10)]
        public string M_LoaiSP { get; set; } // PFK

        [Required]
        [StringLength(10)]
        public string M_GiamGia { get; set; } // PFK

        // Navigation Properties
        [ForeignKey("M_LoaiSP")]
        public virtual LoaiSanPham LoaiSanPham { get; set; }
        [ForeignKey("M_GiamGia")]
        public virtual GiamGia GiamGia { get; set; }
    }
}
