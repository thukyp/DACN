using DACS.Models;
using DACS.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Đảm bảo có using này
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DACS.Areas.QuanLyXNK.Controllers
{
    [Area("QuanLyXNK")]
    [Authorize(Roles = "Owner,QuanLyXNK")]
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ThongKeController> _logger;

        public ThongKeController(ApplicationDbContext context, ILogger<ThongKeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: QuanLyXNK/ThongKe
        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay)
        {
            _logger.LogInformation("Bắt đầu action Thống kê Index. Tham số: Từ Ngày '{TuNgay}', Đến Ngày '{DenNgay}'", tuNgay, denNgay);

            // --- Xử lý khoảng thời gian mặc định ---
            DateTime startDate = tuNgay ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); // Ngày đầu tháng hiện tại
            DateTime endDate = denNgay ?? DateTime.Today; // Ngày hiện tại
            DateTime endDateForQuery = endDate.AddDays(1); // Để bao gồm cả ngày kết thúc trong truy vấn (< endDateForQuery)

            _logger.LogInformation("Khoảng thời gian được xử lý: startDate = {StartDate}, endDate = {EndDate}, endDateForQuery = {EndDateForQuery}", startDate, endDate, endDateForQuery);

            var viewModel = new ThongKeNXViewModel
            {
                TuNgay = startDate,
                DenNgay = endDate,
                ThongKeItems = new List<ThongKeItemViewModel>()
            };

            try
            {
                // --- Lấy danh sách tất cả sản phẩm có liên quan (từ tồn kho, nhập, xuất) ---
                var productsInImports = await _context.ChiTietThuGoms
                                            .Include(ct => ct.YeuCauThuGom)
                                            .Where(ct => (ct.YeuCauThuGom.TrangThai == "Hoàn thành" || ct.YeuCauThuGom.TrangThai == "Thu gom thất bại") && // ĐÃ SỬA
                                                          ct.YeuCauThuGom.ThoiGianHoanThanh >= startDate && ct.YeuCauThuGom.ThoiGianHoanThanh < endDateForQuery)
                                            .Select(ct => new { ct.M_LoaiSP, ct.M_DonViTinh })
                                            .Distinct()
                                            .ToListAsync();
                _logger.LogInformation("Số lượng productsInImports (sau distinct): {Count}", productsInImports.Count);

                var productsInExports = await _context.ChiTietPhieuXuats
                                            .Include(ct => ct.PhieuXuat)
                                            .Where(ct => ct.PhieuXuat.NgayXuat >= startDate && ct.PhieuXuat.NgayXuat < endDateForQuery)
                                            .Select(ct => new { ct.M_LoaiSP, ct.M_DonViTinh })
                                            .Distinct()
                                            .ToListAsync();
                _logger.LogInformation("Số lượng productsInExports (sau distinct): {Count}", productsInExports.Count);

                var productsInStock = await _context.TonKhos
                                            .Select(tk => new { tk.M_LoaiSP, tk.M_DonViTinh })
                                            .Distinct()
                                            .ToListAsync();
                _logger.LogInformation("Số lượng productsInStock (sau distinct): {Count}", productsInStock.Count);

                // Kết hợp và lấy danh sách duy nhất (Mã SP, Mã ĐVT)
                var allRelevantProducts = productsInImports
                    .Union(productsInExports)
                    .Union(productsInStock)
                    .ToList();


                if (!allRelevantProducts.Any())
                {
                    _logger.LogInformation("Không tìm thấy sản phẩm nào liên quan (allRelevantProducts rỗng). Trả về view rỗng.");
                    return View(viewModel); // Trả về view rỗng nếu không có sản phẩm nào
                }

                // --- Lấy thông tin chi tiết Sản phẩm và Đơn vị tính ---
                var productCodes = allRelevantProducts.Select(p => p.M_LoaiSP).Distinct().ToList();
                var unitCodes = allRelevantProducts.Select(p => p.M_DonViTinh).Distinct().ToList();

                var productDetails = await _context.LoaiSanPhams
                                        .Where(lsp => productCodes.Contains(lsp.M_LoaiSP))
                                        .ToDictionaryAsync(lsp => lsp.M_LoaiSP, lsp => lsp.TenLoai);

                var unitDetails = await _context.DonViTinhs
                                        .Where(dvt => unitCodes.Contains(dvt.M_DonViTinh))
                                        .ToDictionaryAsync(dvt => dvt.M_DonViTinh, dvt => dvt.TenLoaiTinh);

                // --- Tính toán Nhập, Xuất, Tồn cho từng sản phẩm ---
                var importsGrouped = await _context.ChiTietThuGoms
                    .Include(ct => ct.YeuCauThuGom)
                    .Where(ct => (ct.YeuCauThuGom.TrangThai == "Hoàn thành" || ct.YeuCauThuGom.TrangThai == "Thu gom thất bại") && // ĐÃ SỬA
                                  ct.YeuCauThuGom.ThoiGianHoanThanh >= startDate && ct.YeuCauThuGom.ThoiGianHoanThanh < endDateForQuery)
                    .GroupBy(ct => new { ct.M_LoaiSP, ct.M_DonViTinh })
                    .Select(g => new { g.Key.M_LoaiSP, g.Key.M_DonViTinh, TongNhap = (long)g.Sum(ct => ct.SoLuong) }) // Ép kiểu sang long nếu SoLuong là int
                    .ToListAsync();
                _logger.LogInformation("Số lượng importsGrouped: {Count}", importsGrouped.Count);
                if (importsGrouped.Any())
                {
                    _logger.LogInformation("Ví dụ importsGrouped: SP {MaSP}, DVT {MaDVT}, TongNhap {Tong}", importsGrouped.First().M_LoaiSP, importsGrouped.First().M_DonViTinh, importsGrouped.First().TongNhap);
                }

                var exportsGrouped = await _context.ChiTietPhieuXuats
                    .Include(ct => ct.PhieuXuat)
                    .Where(ct => ct.PhieuXuat.NgayXuat >= startDate && ct.PhieuXuat.NgayXuat < endDateForQuery)
                    .GroupBy(ct => new { ct.M_LoaiSP, ct.M_DonViTinh })
                    .Select(g => new { g.Key.M_LoaiSP, g.Key.M_DonViTinh, TongXuat = g.Sum(ct => ct.SoLuong) })
                    .ToListAsync();
                _logger.LogInformation("Số lượng exportsGrouped: {Count}", exportsGrouped.Count);


                var stockGrouped = await _context.TonKhos
                    .GroupBy(tk => new { tk.M_LoaiSP, tk.M_DonViTinh })
                    .Select(g => new { g.Key.M_LoaiSP, g.Key.M_DonViTinh, TonHienTai = g.Sum(tk => tk.KhoiLuong) })
                    .ToListAsync();
                _logger.LogInformation("Số lượng stockGrouped: {Count}", stockGrouped.Count);

                // --- Tạo ViewModel List ---
                foreach (var productInfo in allRelevantProducts)
                {
                    var importData = importsGrouped.FirstOrDefault(i => i.M_LoaiSP == productInfo.M_LoaiSP && i.M_DonViTinh == productInfo.M_DonViTinh);
                    var exportData = exportsGrouped.FirstOrDefault(e => e.M_LoaiSP == productInfo.M_LoaiSP && e.M_DonViTinh == productInfo.M_DonViTinh);
                    var stockData = stockGrouped.FirstOrDefault(s => s.M_LoaiSP == productInfo.M_LoaiSP && s.M_DonViTinh == productInfo.M_DonViTinh);

                    viewModel.ThongKeItems.Add(new ThongKeItemViewModel
                    {
                        MaSanPham = productInfo.M_LoaiSP,
                        TenSanPham = productDetails.TryGetValue(productInfo.M_LoaiSP, out var tenSP) ? tenSP ?? productInfo.M_LoaiSP : productInfo.M_LoaiSP,
                        MaDonViTinh = productInfo.M_DonViTinh,
                        TenDonViTinh = unitDetails.TryGetValue(productInfo.M_DonViTinh, out var tenDVT) ? tenDVT ?? productInfo.M_DonViTinh : productInfo.M_DonViTinh,
                        TongNhap = importData?.TongNhap ?? 0,
                        TongXuat = exportData?.TongXuat ?? 0,
                        TonHienTai = stockData?.TonHienTai ?? 0
                    });
                }
                _logger.LogInformation("Đã tạo {Count} ThongKeItemViewModel.", viewModel.ThongKeItems.Count);

                // Sắp xếp kết quả (ví dụ theo tên sản phẩm)
                viewModel.ThongKeItems = viewModel.ThongKeItems.OrderBy(item => item.TenSanPham).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi nghiêm trọng xảy ra trong action Thống kê Index.");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải dữ liệu thống kê. Vui lòng thử lại hoặc liên hệ quản trị viên.";
                // ViewModel vẫn được trả về với ngày tháng đã set, và danh sách ThongKeItems rỗng
            }

            _logger.LogInformation("Kết thúc action Thống kê Index. Trả về View.");
            return View(viewModel);
        }
    }
}