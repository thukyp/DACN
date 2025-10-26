// DACS/Repositories/INguoiMuaRepository.cs
using DACS.Models; // Namespace chứa model NguoiMua
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DACS.Repositories
{
    public interface INguoiMuaRepository
    {
        // Lấy tất cả người mua (có thể thêm phân trang/sắp xếp sau)
        Task<IEnumerable<KhachHang>> GetAllAsync();

        // Lấy người mua theo Mã Người Mua (Khóa chính)
        Task<KhachHang?> GetByMaAsync(string maNguoiMua);

        // Lấy người mua theo UserId (Khóa ngoại từ ApplicationUser) - Rất quan trọng cho Profile
        Task<KhachHang?> GetByUserIdAsync(string userId);

        // Thêm người mua mới
        Task AddAsync(KhachHang nguoiMua);

        // Cập nhật thông tin người mua
        void Update(KhachHang nguoiMua); // Update thường không cần async

        // Xóa người mua
        void Delete(KhachHang nguoiMua); // Delete thường không cần async

        // Lưu các thay đổi vào database (Quan trọng!)
        Task SaveChangesAsync();
    }
}