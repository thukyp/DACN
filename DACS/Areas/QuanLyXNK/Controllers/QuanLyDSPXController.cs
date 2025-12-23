using DACS.Models;
using DACS.Models.ViewModels;
using DACS.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DACS.Areas.QuanLyXNK.Controllers
{
    [Area("QuanLyXNK")]
    [Authorize(Roles = "Owner,QuanLyXNK")]
    public class QuanLyDSPXController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuanLyDSPXController> _logger;

        public QuanLyDSPXController(ApplicationDbContext context, ILogger<QuanLyDSPXController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // --- ACTION INDEX ---
        public async Task<IActionResult> Index(string? searchTerm, string? statusFilter, DateTime? dateFilter, int page = 1)
        {
            int pageSize = 10;
            var query = _context.PhieuXuats
                .Include(px => px.KhoHang)
                .Include(px => px.ChiTietPhieuXuats)
                .AsNoTracking()
                .AsQueryable();

            // 1. Filter
            if (!string.IsNullOrEmpty(searchTerm))
            {
                string lowerSearch = searchTerm.ToLower().Trim();
                query = query.Where(px => (px.NguoiNhan != null && px.NguoiNhan.ToLower().Contains(lowerSearch)) ||
                                          px.MaPhieuXuat.ToString().Contains(lowerSearch));
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
            {
                query = query.Where(px => px.TrangThai == statusFilter);
            }

            if (dateFilter.HasValue)
            {
                query = query.Where(px => px.NgayXuat.Date == dateFilter.Value.Date);
            }

            query = query.OrderByDescending(px => px.NgayXuat);

            // 2. Statistics
            var statsData = await _context.PhieuXuats
                .GroupBy(x => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    DaGiao = g.Count(x => x.TrangThai == PhieuXuatTrangThai.DaXuat),
                    DangXuLy = g.Count(x => x.TrangThai == PhieuXuatTrangThai.DangXuLy || x.TrangThai == PhieuXuatTrangThai.MoiTao),
                    DaHuy = g.Count(x => x.TrangThai == PhieuXuatTrangThai.DaHuy)
                }).FirstOrDefaultAsync();

            var statsModel = new PhieuXuatIndexViewModel
            {
                TongPhieuXuat = statsData?.Total ?? 0,
                DaGiaoCount = statsData?.DaGiao ?? 0,
                DangXuLyCount = statsData?.DangXuLy ?? 0,
                DaHuyCount = statsData?.DaHuy ?? 0
            };

            // 3. Paging
            var totalItems = await query.CountAsync();
            statsModel.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            statsModel.PageIndex = page;
            statsModel.PhieuXuatItems = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(px => new PhieuXuatListItemViewModel
                {
                    MaPhieuXuat = px.MaPhieuXuat,
                    NguoiNhan = px.NguoiNhan,
                    NgayXuat = px.NgayXuat,
                    TenKhoXuat = px.KhoHang != null ? px.KhoHang.TenKho : "",
                    TongSoLuongItems = px.ChiTietPhieuXuats.Sum(ct => ct.SoLuong),
                    TrangThai = px.TrangThai
                }).ToListAsync();

            // 4. Dropdown Status
            statsModel.StatusOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "all", Text = "Tất cả trạng thái" },
                new SelectListItem { Value = PhieuXuatTrangThai.MoiTao, Text = PhieuXuatTrangThai.MoiTao },
                new SelectListItem { Value = PhieuXuatTrangThai.DangXuLy, Text = PhieuXuatTrangThai.DangXuLy },
                new SelectListItem { Value = PhieuXuatTrangThai.DaXuat, Text = PhieuXuatTrangThai.DaXuat },
                new SelectListItem { Value = PhieuXuatTrangThai.DaHuy, Text = PhieuXuatTrangThai.DaHuy }
            };

            var selectedStatus = statsModel.StatusOptions.FirstOrDefault(o => o.Value == statusFilter);
            if (selectedStatus != null) selectedStatus.Selected = true;

            statsModel.SearchTerm = searchTerm;
            statsModel.StatusFilter = statusFilter;
            statsModel.DateFilter = dateFilter;

            return View(statsModel);
        }

        // --- HELPERS ---
        private async Task PopulateDropdownsAsync(PhieuXuatCreateViewModel viewModel)
        {
            viewModel.KhoHangOptions = await _context.KhoHangs
                .Where(kh => kh.TrangThai != KhoHangTrangThai.BaoTri)
                .OrderBy(kh => kh.TenKho)
                .Select(kh => new SelectListItem { Value = kh.MaKho, Text = $"{kh.TenKho} ({kh.MaKho})" })
                .ToListAsync();

            viewModel.SanPhamOptions = await _context.SanPhams
                .OrderBy(sp => sp.TenSanPham)
                .Select(sp => new SelectListItem { Value = sp.M_SanPham, Text = $"{sp.TenSanPham} ({sp.M_SanPham})" })
                .ToListAsync();

            viewModel.DonViTinhOptions = await _context.DonViTinhs
                .OrderBy(dvt => dvt.TenLoaiTinh)
                .Select(dvt => new SelectListItem { Value = dvt.M_DonViTinh, Text = dvt.TenLoaiTinh })
                .ToListAsync();
        }

        // --- CREATE GET ---
        public async Task<IActionResult> Create()
        {
            var viewModel = new PhieuXuatCreateViewModel
            {
                NgayXuat = DateTime.Now,
                ChiTietItems = new List<ChiTietPhieuXuatItemViewModel> { new ChiTietPhieuXuatItemViewModel() }
            };
            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        // --- CREATE POST ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhieuXuatCreateViewModel viewModel)
        {
            // 1. Kiểm tra logic nghiệp vụ: Phải có ít nhất 1 dòng hợp lệ
            var validItems = viewModel.ChiTietItems?
                .Where(ct => !string.IsNullOrEmpty(ct.M_LoaiSP) && ct.SoLuong > 0)
                .ToList();

            if (validItems == null || !validItems.Any())
            {
                ModelState.AddModelError("ChiTietItems", "Vui lòng chọn ít nhất một sản phẩm hợp lệ.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 2. Load dữ liệu Tồn kho 1 lần để tối ưu (Có Trim ID để tránh lỗi khoảng trắng)
                var productIds = validItems.Select(x => x.M_LoaiSP.Trim()).Distinct().ToList();
                var unitIds = validItems.Select(x => x.M_DonViTinh.Trim()).Distinct().ToList();

                var inventoryRecords = await _context.LoTonKhos
                    .Where(tk => tk.MaKho == viewModel.MaKho
                                 && productIds.Contains(tk.M_SanPham)
                                 && unitIds.Contains(tk.M_DonViTinh))
                    .ToListAsync();

                // 3. Tạo Header Phiếu Xuất
                var phieuXuat = new PhieuXuat
                {
                    NgayXuat = viewModel.NgayXuat,
                    MaKho = viewModel.MaKho,
                    LyDoXuat = viewModel.LyDoXuat,
                    TrangThai = PhieuXuatTrangThai.MoiTao,
                    ChiTietPhieuXuats = new List<ChiTietPhieuXuat>()
                };

                // 4. Xử lý từng dòng chi tiết
                foreach (var item in validItems)
                {
                    // Lấy mã SP và ĐVT, xóa khoảng trắng thừa
                    string itemMaSP = item.M_LoaiSP.Trim();
                    string itemMaDVT = item.M_DonViTinh.Trim();

                    // Tìm lô tồn kho (So sánh chính xác cả SP và ĐVT sau khi Trim)
                    // Lưu ý: inventoryRecords đã load lên RAM nên dùng .Trim() ở đây thoải mái
                    var stockRecord = inventoryRecords.FirstOrDefault(tk =>
                        tk.M_SanPham.Trim() == itemMaSP &&
                        tk.M_DonViTinh.Trim() == itemMaDVT);

                    if (stockRecord == null)
                    {
                        // --- DEBUG LOGIC: Kiểm tra kỹ xem lỗi do đâu ---
                        var existProductOnly = inventoryRecords.Any(tk => tk.M_SanPham.Trim() == itemMaSP);

                        if (existProductOnly)
                        {
                            // Tìm thấy sản phẩm nhưng khác ĐVT -> Báo lỗi cho người dùng biết
                            // Ví dụ: Kho có "Kg" nhưng người dùng chọn "Cái"
                            throw new Exception($"Sản phẩm '{itemMaSP}' có trong kho nhưng KHÁC ĐƠN VỊ TÍNH. " +
                                                $"Bạn đang chọn '{itemMaDVT}', vui lòng kiểm tra lại tồn kho.");
                        }
                        else
                        {
                            // Không tìm thấy sản phẩm này trong kho này
                            throw new Exception($"Sản phẩm '{itemMaSP}' hoàn toàn không có trong kho '{viewModel.MaKho}'.");
                        }
                    }

                    // Kiểm tra số lượng tồn
                    if (stockRecord.KhoiLuongConLai < item.SoLuong)
                    {
                        throw new Exception($"Sản phẩm '{itemMaSP}' không đủ hàng. Tồn: {stockRecord.KhoiLuongConLai}, Xuất: {item.SoLuong}");
                    }

                    // --- TRỪ KHO ---
                    stockRecord.KhoiLuongConLai -= item.SoLuong;

                    // QUAN TRỌNG: Ép buộc Entity Framework đánh dấu object này là "Đã sửa" (Modified)
                    _context.Entry(stockRecord).State = EntityState.Modified;

                    // Thêm vào phiếu xuất
                    phieuXuat.ChiTietPhieuXuats.Add(new ChiTietPhieuXuat
                    {
                        M_SanPham = itemMaSP, // Lưu mã đã trim
                        M_DonViTinh = itemMaDVT, // Lưu mã đã trim
                        SoLuong = item.SoLuong
                    });
                }

                _context.PhieuXuats.Add(phieuXuat);

                // Lưu tất cả thay đổi
                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Tạo phiếu xuất thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi khi tạo phiếu xuất");

                // Hiển thị lỗi chi tiết ra màn hình
                ModelState.AddModelError("", ex.Message);

                await PopulateDropdownsAsync(viewModel);
                return View(viewModel);
            }
        }
    }
}