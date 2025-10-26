using DACS.Models; // Namespace chứa Models (YeuCauThuGom, ChiTietThuGom, LoaiSanPham, DonViTinh)
using DACS.Models.ViewModels;
using Microsoft.EntityFrameworkCore; // Cần cho EF Core operations (Include, ThenInclude, FirstOrDefaultAsync, etc.)
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DACS.Repositories // Namespace chứa Repositories
{
    public class ThuGomRepository : IThuGomRepository
    {
        private readonly ApplicationDbContext _context;

        // Constructor để inject DbContext
        public ThuGomRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Thêm một YeuCauThuGom mới.
        /// Lưu ý: Phương thức này chỉ đánh dấu Add, việc SaveChanges cần được gọi ở nơi khác (Controller/Service/UnitOfWork).
        /// </summary>
        public async Task AddAsync(YeuCauThuGom yeuCauThuGom)
        {
            // Thêm yêu cầu vào DbContext, bao gồm cả ChiTietThuGoms đi kèm (do quan hệ điều hướng)
            await _context.YeuCauThuGoms.AddAsync(yeuCauThuGom);
            // Không gọi _context.SaveChangesAsync() ở đây theo như cách Controller đang hoạt động
        }

        /// <summary>
        /// Xóa một YeuCauThuGom theo ID.
        /// Lưu ý: Cần SaveChanges sau khi gọi. Xem xét logic thực tế (chỉ đổi trạng thái?).
        /// </summary>
        public async Task DeleteAsync(string id)
        {
            var yeuCau = await _context.YeuCauThuGoms.FindAsync(id);
            if (yeuCau != null)
            {
                _context.YeuCauThuGoms.Remove(yeuCau);
                // Không gọi _context.SaveChangesAsync() ở đây
            }
            // Cân nhắc: Nếu logic chỉ là đổi trạng thái thành "Đã hủy", nên dùng phương thức UpdateAsync
        }

        /// <summary>
        /// Lấy tất cả YeuCauThuGom (thường dùng cho Admin), bao gồm chi tiết cơ bản.
        /// </summary>
        public async Task<IEnumerable<YeuCauThuGom>> GetAllAsync()
        {
            // Bao gồm thông tin Khách Hàng và Chi Tiết cơ bản để hiển thị danh sách tổng quan
            return await _context.YeuCauThuGoms
                .Include(yc => yc.KhachHang) // Lấy thông tin khách hàng liên quan
                .Include(yc => yc.ChiTietThuGoms) // Lấy danh sách chi tiết
                    .ThenInclude(ct => ct.LoaiSanPham) // Từ chi tiết lấy Loại Sản Phẩm
                .OrderByDescending(yc => yc.NgayYeuCau) // Sắp xếp mới nhất lên đầu
                .AsNoTracking() // Dùng AsNoTracking vì chỉ đọc dữ liệu
                .ToListAsync();
        }

        public Task<IEnumerable<YeuCauThuGom>> GetAllAsync(bool includeRelated = true)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lấy tất cả YeuCauThuGom của một Khách hàng, bao gồm chi tiết cần thiết để hiển thị lịch sử.
        /// </summary>
        public async Task<IEnumerable<YeuCauThuGom>> GetAllForKhachHangAsync(string maKhachHang)
        {
            return await _context.YeuCauThuGoms
                .Where(yc => yc.M_KhachHang == maKhachHang) // Lọc theo mã khách hàng
                .Include(yc => yc.ChiTietThuGoms)          // Include ChiTietThuGom
                    .ThenInclude(ct => ct.LoaiSanPham)     // SỬA Ở ĐÂY: Include LoaiSanPham từ ChiTietThuGom
                .Include(yc => yc.ChiTietThuGoms)          // Include lại ChiTietThuGom để lấy tiếp DonViTinh
                    .ThenInclude(ct => ct.DonViTinh)       // Include DonViTinh từ ChiTietThuGom
                .OrderByDescending(yc => yc.NgayYeuCau)    // Sắp xếp mới nhất lên đầu
                .AsNoTracking()                            // Dùng AsNoTracking vì chỉ đọc dữ liệu
                .ToListAsync();
        }

        /// <summary>
        /// Lấy YeuCauThuGom theo ID, có tùy chọn bao gồm dữ liệu liên quan chi tiết.
        /// </summary>
        public async Task<YeuCauThuGom?> GetByIdAsync(string id, bool includeDetails = false)
        {
            IQueryable<YeuCauThuGom> query = _context.YeuCauThuGoms;

            if (includeDetails)
            {
                query = query
                    .Include(yc => yc.KhachHang)            // Thông tin khách hàng
                    .Include(yc => yc.ChiTietThuGoms)       // Danh sách chi tiết
                        .ThenInclude(ct => ct.LoaiSanPham) // SỬA Ở ĐÂY: Loại sản phẩm từ chi tiết
                    .Include(yc => yc.ChiTietThuGoms)       // Lấy lại chi tiết để ThenInclude tiếp
                        .ThenInclude(ct => ct.DonViTinh);  // Đơn vị tính từ chi tiết
                                                           // Thêm các Include/ThenInclude khác nếu cần
            }
            return await query.FirstOrDefaultAsync(yc => yc.M_YeuCau == id);
        }

        public Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Cập nhật một YeuCauThuGom.
        /// Lưu ý: Phương thức này chỉ đánh dấu Update, việc SaveChanges cần được gọi ở nơi khác.
        /// </summary>
        public Task UpdateAsync(YeuCauThuGom yeuCauThuGom)
        {
            // Đánh dấu toàn bộ entity là Modified
            _context.YeuCauThuGoms.Update(yeuCauThuGom);
            // Không gọi _context.SaveChangesAsync() ở đây
            return Task.CompletedTask; // Update là thao tác đồng bộ với DbContext
        }
    }
}