using DACS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DACS.Repositories // Hoặc namespace bạn muốn
{
    public interface ISanPhamRepository
    {
        Task<IEnumerable<SanPham>> GetAllAsync();
        Task<SanPham?> GetByIdAsync(string id); // Dùng ? nếu có thể null
        Task AddAsync(SanPham entity);
        Task UpdateAsync(SanPham entity);
        Task DeleteAsync(string id);
        Task<int> GetMaxNumericIdAsync(); // Thêm hàm lấy số lớn nhất
        Task<bool> ExistsAsync(string id);
        // Thêm các phương thức truy vấn khác nếu cần
    }
}