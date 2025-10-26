using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class AnhSanPham
    {
        [Key]
        public int MaAnh { get; set; }  // Khóa chính

        [Required]
        [StringLength(255)]
        public string TenPhuPham { get; set; }  // Tên file ảnh

        [Required]
        [StringLength(500)]
        public string DuongDan { get; set; }  // Đường dẫn lưu ảnh (VD: /uploads/sanpham/abc.jpg)


        [Required]
        [StringLength(10)]
        public string M_SanPham { get; set; }
        [ForeignKey("M_SanPham")]
        public virtual SanPham SanPham { get; set; }
    }
}
