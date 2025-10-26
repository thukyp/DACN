using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class LoaiSanPham // Product Type / Category
    {
        [Key]
        [StringLength(10)]
        public string M_LoaiSP { get; set; }

        [Required(ErrorMessage = "Tên loại là bắt buộc.")]
        [StringLength(100)]
        public string TenLoai { get; set; }

        // Navigation Property
        public virtual ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();


    }

}
