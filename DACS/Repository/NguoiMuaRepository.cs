// DACS/Repositories/NguoiMuaRepository.cs
using DACS.Models;
using Microsoft.EntityFrameworkCore; // Cần cho Entity Framework Core
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DACS.Repositories
{
    public class NguoiMuaRepository : INguoiMuaRepository
    {
        private readonly ApplicationDbContext _context;

        // Inject DbContext vào Repository
        public NguoiMuaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(KhachHang nguoiMua)
        {
            await _context.KhachHangs.AddAsync(nguoiMua);
            // Chưa gọi SaveChangesAsync() ở đây
        }

        public void Delete(KhachHang nguoiMua)
        {
            _context.KhachHangs.Remove(nguoiMua);
            // Chưa gọi SaveChangesAsync() ở đây
        }

        public async Task<IEnumerable<KhachHang>> GetAllAsync()
        {
            // Có thể thêm .Include() nếu muốn lấy kèm dữ liệu liên quan
            return await _context.KhachHangs.ToListAsync();
        }

        public async Task<KhachHang?> GetByMaAsync(string maNguoiMua)
        {
            return await _context.KhachHangs
                                 // .Include(nm => nm.User) // Ví dụ Include User nếu cần
                                 .FirstOrDefaultAsync(nm => nm.M_KhachHang == maNguoiMua);
        }

        public async Task<KhachHang?> GetByUserIdAsync(string userId)
        {
            return await _context.KhachHangs
                                 // .Include(nm => nm.User) // Ví dụ Include User nếu cần
                                 .FirstOrDefaultAsync(nm => nm.UserId == userId);
        }

        public void Update(KhachHang nguoiMua)
        {
            _context.KhachHangs.Update(nguoiMua);
            // Chưa gọi SaveChangesAsync() ở đây
        }

        // Phương thức quan trọng để lưu thay đổi
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}