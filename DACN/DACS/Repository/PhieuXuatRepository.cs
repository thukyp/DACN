// File: Repositories/PhieuXuatRepository.cs
using DACS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Thêm using cho ILogger
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DACS.Repositories
{
    public class PhieuXuatRepository : IPhieuXuatRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PhieuXuatRepository> _logger;

        public PhieuXuatRepository(ApplicationDbContext context, ILogger<PhieuXuatRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddPhieuXuatAsync(PhieuXuat phieuXuat)
        {
            if (phieuXuat == null) throw new ArgumentNullException(nameof(phieuXuat));
            if (phieuXuat.ChiTietPhieuXuats == null || !phieuXuat.ChiTietPhieuXuats.Any())
            {
                throw new InvalidOperationException("Phiếu xuất phải có ít nhất một chi tiết.");
            }
            if (string.IsNullOrWhiteSpace(phieuXuat.MaKho))
            {
                throw new InvalidOperationException("Chưa chọn kho xuất hàng cho phiếu xuất.");
            }

            // Bắt đầu Transaction để đảm bảo tính toàn vẹn
            using var transaction = await _context.Database.BeginTransactionAsync();
            _logger.LogInformation("Bắt đầu transaction thêm Phiếu xuất ngày {NgayXuat} từ kho {MaKho}.", phieuXuat.NgayXuat, phieuXuat.MaKho);

            try
            {
                // 1. Thêm Phiếu xuất chính vào context (chưa lưu DB)
                _context.PhieuXuats.Add(phieuXuat);
                // EF Core sẽ tự động thêm các ChiTietPhieuXuats nếu chúng có trong collection của phieuXuat

                // 2. Kiểm tra và Cập nhật Tồn kho cho từng chi tiết
                foreach (var detail in phieuXuat.ChiTietPhieuXuats)
                {
                    if (detail.SoLuong <= 0)
                    {
                        throw new InvalidOperationException($"Số lượng xuất của sản phẩm {detail.M_LoaiSP} phải lớn hơn 0.");
                    }

                    // Tìm bản ghi tồn kho tương ứng
                    var tonKhoRecord = await _context.TonKhos
                        .FirstOrDefaultAsync(tk => tk.MaKho == phieuXuat.MaKho && // Lấy từ MaKho của PhieuXuat
                                               tk.M_LoaiSP == detail.M_LoaiSP &&
                                               tk.M_DonViTinh == detail.M_DonViTinh);

                    if (tonKhoRecord == null)
                    {
                        throw new InvalidOperationException($"Không tìm thấy tồn kho cho sản phẩm {detail.M_LoaiSP} ({detail.M_DonViTinh}) tại kho {phieuXuat.MaKho}.");
                    }

                    // Kiểm tra số lượng tồn
                    if (tonKhoRecord.KhoiLuong < detail.SoLuong)
                    {
                        // Lấy tên SP để báo lỗi rõ ràng hơn
                        var tenSP = await _context.LoaiSanPhams
                                           .Where(sp => sp.M_LoaiSP == detail.M_LoaiSP)
                                           .Select(sp => sp.TenLoai)
                                           .FirstOrDefaultAsync();
                        throw new InvalidOperationException($"Không đủ số lượng tồn kho cho '{tenSP ?? detail.M_LoaiSP}' ({detail.M_DonViTinh}) tại kho {phieuXuat.MaKho}. Tồn: {tonKhoRecord.KhoiLuong}, Xuất: {detail.SoLuong}.");
                    }

                    // Giảm số lượng tồn kho
                    tonKhoRecord.KhoiLuong -= detail.SoLuong;
                    _context.TonKhos.Update(tonKhoRecord); // Đánh dấu để cập nhật
                    _logger.LogInformation("Chuẩn bị cập nhật TonKho: Kho={MaKho}, SP={MaSP}, DVT={MaDVT}. Số lượng -{SoLuong}. Tồn mới: {TonMoi}",
                                           phieuXuat.MaKho, detail.M_LoaiSP, detail.M_DonViTinh, detail.SoLuong, tonKhoRecord.KhoiLuong);
                }

                // 3. Lưu tất cả thay đổi vào DB (Add PhieuXuat, Add ChiTiet, Update TonKho)
                await _context.SaveChangesAsync(); // Chỉ gọi SaveChanges một lần ở cuối

                // 4. Commit transaction nếu mọi thứ thành công
                await transaction.CommitAsync();
                _logger.LogInformation("Đã commit transaction thành công cho Phiếu xuất ngày {NgayXuat} từ kho {MaKho}.", phieuXuat.NgayXuat, phieuXuat.MaKho);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, rollback transaction
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi thêm Phiếu xuất ngày {NgayXuat} từ kho {MaKho}. Transaction đã được rollback.", phieuXuat.NgayXuat, phieuXuat.MaKho);
                // Ném lại lỗi để Controller hoặc lớp gọi có thể xử lý
                throw;
            }
        }


        public async Task<PhieuXuat?> GetPhieuXuatByIdAsync(int maPhieuXuat, bool includeDetails = true)
        {
            var query = _context.PhieuXuats.AsQueryable();

            if (includeDetails)
            {
                query = query.Include(px => px.ChiTietPhieuXuats)! // Dùng ! để báo EF Core biết sẽ có details
                           .ThenInclude(ct => ct.LoaiSanPham)
                       .Include(px => px.ChiTietPhieuXuats)!
                           .ThenInclude(ct => ct.DonViTinh)
                       .Include(px => px.KhoHang); // Include Kho hàng xuất
            }

            return await query.FirstOrDefaultAsync(px => px.MaPhieuXuat == maPhieuXuat);
        }

        public async Task<(IEnumerable<PhieuXuat> Items, int TotalCount)> GetPagedPhieuXuatAsync(
            DateTime? tuNgay, DateTime? denNgay, int pageIndex, int pageSize, bool trackChanges = false)
        {
            var query = _context.PhieuXuats.AsQueryable();

            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            // Áp dụng bộ lọc ngày
            if (tuNgay.HasValue)
            {
                query = query.Where(px => px.NgayXuat.Date >= tuNgay.Value.Date);
            }
            if (denNgay.HasValue)
            {
                // Bao gồm cả ngày kết thúc
                DateTime endDateValue = denNgay.Value.Date.AddDays(1);
                query = query.Where(px => px.NgayXuat < endDateValue);
            }

            // Lấy tổng số lượng trước khi phân trang
            var totalItems = await query.CountAsync();

            // Sắp xếp và Phân trang
            var items = await query
               .Include(px => px.KhoHang) // Lấy tên kho
               .OrderByDescending(px => px.NgayXuat) // Mới nhất lên đầu
               .Skip((pageIndex - 1) * pageSize)
               .Take(pageSize)
               .ToListAsync();

            return (items, totalItems);
        }
    }
}