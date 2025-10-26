using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models.ViewModels
{
    public class PhieuXuatIndexViewModel
    {
        public List<PhieuXuatListItemViewModel> PhieuXuatItems { get; set; } = new List<PhieuXuatListItemViewModel>();

        // Thông tin cho stat cards
        public int TongPhieuXuat { get; set; }
        public int DaGiaoCount { get; set; }
        public int DangXuLyCount { get; set; }
        public int DaHuyCount { get; set; }

        // Bộ lọc
        [Display(Name = "Tìm kiếm")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Trạng thái")]
        public string? StatusFilter { get; set; }
        public IEnumerable<SelectListItem>? StatusOptions { get; set; }

        [Display(Name = "Ngày xuất")]
        [DataType(DataType.Date)]
        public DateTime? DateFilter { get; set; }

        // Phân trang
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}