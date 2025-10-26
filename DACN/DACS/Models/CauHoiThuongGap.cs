using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    [Table("CauHoiThuongGap")]
    public class CauHoiThuongGap
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Intent { get; set; } // Ví dụ: "ban_san_pham", "hoi_gia", "dat_lich"

        [Required]
        [StringLength(255)]
        public string TrainingSentence { get; set; } // Câu mẫu huấn luyện

        [Required]
        [StringLength(255)]
        public string Response { get; set; } // Câu trả lời mẫu

        [StringLength(100)]
        public string? ProductName { get; set; }
    }
    }

