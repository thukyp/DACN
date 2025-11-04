using System.Collections.Generic;

namespace DACS.Models.ViewModels
{
    public class SanPhamDetailsViewModel
    {
        public SanPham SanPham { get; set; }
        public List<LoTonKho> AvailableLots { get; set; }
        public decimal TotalStock { get; set; }
    }
}