using DACS.Models;
using DACS.Models.ViewModels; // Cần cho ViewModel nếu Repository trả về ViewModel
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DACS.Repositories
{
    public interface IThuGomRepository
    {
        // Lấy một yêu cầu theo ID, tùy chọn include chi tiết
        Task<YeuCauThuGom?> GetByIdAsync(string id, bool includeDetails = false);

        // Lấy tất cả yêu cầu (cho Admin), tùy chọn include
        Task<IEnumerable<YeuCauThuGom>> GetAllAsync(bool includeRelated = true);

        // Lấy tất cả yêu cầu của một khách hàng
        Task<IEnumerable<YeuCauThuGom>> GetAllForKhachHangAsync(string maKhachHang);


        // Thêm một yêu cầu mới (chỉ Add, chưa Save)
        Task AddAsync(YeuCauThuGom yeuCauThuGom);

        // Cập nhật một yêu cầu (chỉ đánh dấu Update, chưa Save)
        Task UpdateAsync(YeuCauThuGom yeuCauThuGom);

        // Xóa một yêu cầu (chỉ đánh dấu Remove, chưa Save)
        Task DeleteAsync(string id);

        // Lưu các thay đổi vào CSDL (có thể bỏ nếu dùng UnitOfWork)
        Task SaveChangesAsync();
    }
}