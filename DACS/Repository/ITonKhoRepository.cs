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
        Task<(IEnumerable<TonKho> Items, int TotalCount)> GetPagedTonKhoAsync(
            string? searchTerm,
            string? maKhoFilter,
            int pageIndex,
            int pageSize,
            bool trackChanges = false);

        // Lấy danh sách Kho hàng cho dropdown filter
        Task<IEnumerable<SelectListItem>> GetKhoHangOptionsAsync();
        Task UpdateTonKhoKhoiLuongAsync(string productId, float newKhoiLuong);
        Task<TonKho?> GetTonKhoByProductIdAsync(string productId);
        // Có thể thêm các phương thức khác nếu cần (GetById, Add, Update, Delete...)
        // Task<TonKho?> GetByIdAsync(int id, bool trackChanges = false);
        // Task AddAsync(TonKho tonKho);
        // Task UpdateAsync(TonKho tonKho);
        // Task DeleteAsync(int id);

        // Có thể thêm phương thức lấy Định Mức Tối Thiểu (nếu bạn lưu nó ở bảng khác)
        // Task<long?> GetDinhMucToiThieuAsync(string maLoaiSP);
    }
}