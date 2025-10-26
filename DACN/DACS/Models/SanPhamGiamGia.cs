using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class SanPhamGiamGia // ProductDiscount (Junction Table)
    {
        [Required]
        [StringLength(10)]
        public string M_SanPham { get; set; } // PFK

        [Required]
        [StringLength(10)]
        public string M_GiamGia { get; set; } // PFK

        // Navigation Properties
        [ForeignKey("M_SanPham")]
        public virtual SanPham SanPham { get; set; }
        [ForeignKey("M_GiamGia")]
        public virtual GiamGia GiamGia { get; set; }
    }

}
