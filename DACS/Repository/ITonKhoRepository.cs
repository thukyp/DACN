// File: Repositories/ITonKhoRepository.cs (Hoặc Interfaces/ITonKhoRepository.cs)
using DACS.Models;
using DACS.Models.ViewModels; // Cần cho ViewModel nếu phương thức trả về ViewModel
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DACS.Repository // Hoặc DACS.Interfaces
{
    public interface ITonKhoRepository
    {
        // Lấy dữ liệu tồn kho phân trang và lọc
        Task<(IEnumerable<LoTonKho> Items, int TotalCount)> GetPagedTonKhoAsync(
            string? searchTerm,
            string? maKhoFilter,
            int pageIndex,
            int pageSize,
            bool trackChanges = false);

        // Lấy danh sách Kho hàng cho dropdown filter
        Task<IEnumerable<SelectListItem>> GetKhoHangOptionsAsync();
        Task UpdateTonKhoKhoiLuongAsync(string productId, float newKhoiLuong);
        Task<LoTonKho?> GetTonKhoByProductIdAsync(string productId);
    }
}