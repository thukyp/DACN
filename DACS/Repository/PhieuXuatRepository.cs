// File: Repositories/PhieuXuatRepository.cs
using DACS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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

        // <<< ================= VIẾT LẠI HOÀN TOÀN HÀM NÀY ================= >>>
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

            using var transaction = await _context.Database.BeginTransactionAsync();
            _logger.LogInformation("Bắt đầu transaction thêm Phiếu xuất ngày {NgayXuat} từ kho {MaKho}.", phieuXuat.NgayXuat, phieuXuat.MaKho);

            try
            {
                // 1. Thêm Phiếu xuất chính vào context (chưa lưu DB)
                _context.PhieuXuats.Add(phieuXuat);

                // 2. Kiểm tra và Cập nhật Tồn kho (FIFO) cho từng chi tiết
                foreach (var detail in phieuXuat.ChiTietPhieuXuats)
                {
                    if (detail.SoLuong <= 0)
                    {
                        throw new InvalidOperationException($"Số lượng xuất của sản phẩm {detail.M_SanPham} phải lớn hơn 0.");
                    }

                    decimal soLuongCanXuat = (decimal)detail.SoLuong; // Dùng decimal cho nhất quán

                    // --- Logic FIFO Bắt đầu ---

                    // A. Kiểm tra tổng tồn kho trước
                    var tongTonKho = await _context.LoTonKhos
                        .Where(tk => tk.MaKho == phieuXuat.MaKho &&
                                     tk.M_SanPham == detail.M_SanPham &&
                                     tk.M_DonViTinh == detail.M_DonViTinh &&
                                     tk.KhoiLuongConLai > 0)
                        .SumAsync(tk => tk.KhoiLuongConLai);

                    if (tongTonKho < soLuongCanXuat)
                    {
                        var tenSP = await _context.SanPhams // <<< SỬA: Lấy từ SanPhams
                                     .Where(sp => sp.M_SanPham == detail.M_SanPham) // <<< SỬA
                                     .Select(sp => sp.TenSanPham) // <<< SỬA
                                     .FirstOrDefaultAsync();
                        throw new InvalidOperationException($"Không đủ số lượng tồn kho cho '{tenSP ?? detail.M_SanPham}' ({detail.M_DonViTinh}) tại kho {phieuXuat.MaKho}. Tồn: {tongTonKho}, Xuất: {soLuongCanXuat}.");
                    }

                    // B. Lấy các lô hàng theo FIFO (Cũ nhất trước)
                    var availableLots = await _context.LoTonKhos
                        .Where(tk => tk.MaKho == phieuXuat.MaKho &&
                                     tk.M_SanPham == detail.M_SanPham &&
                                     tk.M_DonViTinh == detail.M_DonViTinh &&
                                     tk.KhoiLuongConLai > 0)
                        .OrderBy(tk => tk.NgayNhapKho) // Sắp xếp FIFO
                        .ToListAsync();

                    // C. Trừ kho lần lượt
                    foreach (var lot in availableLots)
                    {
                        if (soLuongCanXuat <= 0) break; // Đã lấy đủ

                        decimal soLuongLayTuLoNay = Math.Min(soLuongCanXuat, lot.KhoiLuongConLai);

                        lot.KhoiLuongConLai -= soLuongLayTuLoNay;
                        soLuongCanXuat -= soLuongLayTuLoNay;

                        _context.LoTonKhos.Update(lot); // <<< SỬA: Dùng LoTonKhos
                        _logger.LogInformation("Trừ kho FIFO: Kho={MaKho}, SP={MaSP}, Lô={MaLo}, Số lượng -{SoLuong}. Tồn lô mới: {TonMoi}",
                                               phieuXuat.MaKho, detail.M_SanPham, lot.MaLoTonKho, soLuongLayTuLoNay, lot.KhoiLuongConLai);

                        // (Nếu cần truy vết chi tiết: "Phiếu xuất A lấy 50kg từ Lô X và 20kg từ Lô Y", 
                        // bạn cần thêm một bảng trung gian ChiTietPhieuXuat_LoTonKho ở đây)
                    }
                    // --- Logic FIFO Kết thúc ---
                }

                // 3. Lưu tất cả thay đổi vào DB
                await _context.SaveChangesAsync();

                // 4. Commit transaction
                await transaction.CommitAsync();
                _logger.LogInformation("Đã commit transaction thành công cho Phiếu xuất ngày {NgayXuat} từ kho {MaKho}.", phieuXuat.NgayXuat, phieuXuat.MaKho);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi thêm Phiếu xuất ngày {NgayXuat} từ kho {MaKho}. Transaction đã được rollback.", phieuXuat.NgayXuat, phieuXuat.MaKho);
                throw;
            }
        }


        public async Task<PhieuXuat?> GetPhieuXuatByIdAsync(int maPhieuXuat, bool includeDetails = true)
        {
            var query = _context.PhieuXuats.AsQueryable();

            if (includeDetails)
            {
                // <<< SỬA: Sửa lại logic Include cho ChiTietPhieuXuat
                query = query
                    .Include(px => px.KhoHang) // Include Kho hàng xuất
                    .Include(px => px.ChiTietPhieuXuats)
                        .ThenInclude(ct => ct.SanPham) // Nối đến SanPham (dựa trên M_SanPham)
                            .ThenInclude(sp => sp.LoaiSanPham) // Nối tiếp đến LoaiSanPham
                    .Include(px => px.ChiTietPhieuXuats)
                        .ThenInclude(ct => ct.DonViTinh); // Nối đến DonViTinh (dựa trên M_DonViTinh)
            }

            return await query.FirstOrDefaultAsync(px => px.MaPhieuXuat == maPhieuXuat);
        }

        public async Task<(IEnumerable<PhieuXuat> Items, int TotalCount)> GetPagedPhieuXuatAsync(
            DateTime? tuNgay, DateTime? denNgay, int pageIndex, int pageSize, bool trackChanges = false)
        {
            // (Logic phân trang của bạn đã đúng, giữ nguyên)
            var query = _context.PhieuXuats.AsQueryable();

            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            if (tuNgay.HasValue)
            {
                query = query.Where(px => px.NgayXuat.Date >= tuNgay.Value.Date);
            }
            if (denNgay.HasValue)
            {
                DateTime endDateValue = denNgay.Value.Date.AddDays(1);
                query = query.Where(px => px.NgayXuat < endDateValue);
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .Include(px => px.KhoHang)
                .OrderByDescending(px => px.NgayXuat)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalItems);
        }
    }
}