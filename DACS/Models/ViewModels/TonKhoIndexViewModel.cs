using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace DACS.Models.ViewModels
{
    public class TonKhoIndexViewModel
    {
        public List<TonKhoListItemViewModel> TonKhoItems { get; set; } = new List<TonKhoListItemViewModel>();

        // Dùng cho bộ lọc
        public string? SearchTerm { get; set; }
        public string? MaKhoFilter { get; set; }
        public List<SelectListItem> KhoHangOptions { get; set; } = new List<SelectListItem>();

        // Dùng cho phân trang
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}