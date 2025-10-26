using DACS.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DACS.Repositories
{
    public class EFSanPhamRepository : ISanPhamRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EFSanPhamRepository> _logger; // Nên có logger

        public EFSanPhamRepository(ApplicationDbContext context, ILogger<EFSanPhamRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<SanPham>> GetAllAsync()
        {
            return await _context.SanPhams
                                 .Include(s => s.LoaiSanPham)
                                 .Include(s => s.DonViTinh)
                                 .ToListAsync();
        }

        public async Task<SanPham?> GetByIdAsync(string id)
        {
            return await _context.SanPhams
                                 .Include(s => s.LoaiSanPham)
                                 .Include(s => s.DonViTinh)
                                 .FirstOrDefaultAsync(m => m.M_SanPham == id);
        }


        public async Task AddAsync(SanPham entity)
        {
            _context.SanPhams.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(SanPham entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _context.SanPhams.FindAsync(id);
            if (entity != null)
            {
                _context.SanPhams.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            return await _context.SanPhams.AnyAsync(e => e.M_SanPham == id);
        }

        public async Task<int> GetMaxNumericIdAsync()
        {
            int maxNumber = 0;
            try
            {
                // 1. Lấy tất cả các chuỗi có khả năng là phần số ("xxx") từ CSDL về bộ nhớ trước
                var potentialNumericParts = await _context.SanPhams
                                                .Where(s => s.M_SanPham.StartsWith("SP") && s.M_SanPham.Length > 2)
                                                .Select(s => s.M_SanPham.Substring(2)) // Chỉ lấy phần chuỗi sau "SP"
                                                .ToListAsync(); // <<< Quan trọng: Lấy dữ liệu về bộ nhớ (List<string>)

                // 2. Lọc và chuyển đổi các chuỗi hợp lệ thành số int TRONG BỘ NHỚ
                var validNumbers = potentialNumericParts
                                    // Dùng LINQ to Objects: kiểm tra chuỗi không rỗng VÀ tất cả ký tự là số
                                    .Where(numStr => !string.IsNullOrEmpty(numStr) && numStr.All(char.IsDigit))
                                    .Select(int.Parse); // Chuyển chuỗi số hợp lệ thành int

                // 3. Tìm số lớn nhất từ danh sách số hợp lệ
                if (validNumbers.Any()) // Kiểm tra xem có số hợp lệ nào không
                {
                    maxNumber = validNumbers.Max(); // Lấy số lớn nhất
                }
                // Nếu không tìm thấy mã SP hợp lệ nào, maxNumber vẫn là 0
            }
            catch (Exception ex) // Bắt các lỗi có thể xảy ra (kết nối DB, parse int,...)
            {
                _logger.LogError(ex, "Lỗi khi truy vấn hoặc xử lý mã sản phẩm lớn nhất trong Repository.");
                // Ném lại lỗi để Controller có thể bắt và xử lý, hoặc bạn có thể return 0/giá trị mặc định khác
                throw;
            }
            return maxNumber;
        }


    }
}