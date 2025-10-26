using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models.ViewModels
{
    public class ThongKeNXViewModel
    {
        [DataType(DataType.Date)]
        [Display(Name = "Từ ngày")]
        public DateTime? TuNgay { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Đến ngày")]
        public DateTime? DenNgay { get; set; }

        public List<ThongKeItemViewModel> ThongKeItems { get; set; } = new List<ThongKeItemViewModel>();
    }
}