using System;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models.ViewModels // Hoặc namespace của bạn
{
    public class ScheduleViewModel
    {
        [Required]
        public string M_YeuCau { get; set; }

        // Chỉ để hiển thị context trên view
        public string? TenKhachHang { get; set; }
        public string? DiaChiTomTat { get; set; }


        [Required(ErrorMessage = "Vui lòng chọn thời gian dự kiến.")]
        [Display(Name = "Thời gian thu gom dự kiến")]
        [DataType(DataType.DateTime)]
        public DateTime ThoiGianSanSang { get; set; }
    }
}