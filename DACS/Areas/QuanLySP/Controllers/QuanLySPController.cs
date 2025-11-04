using DACS.Models;
using DACS.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic; // <<< THÊM USING NÀY
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace DACS.Areas.QuanLySP.Controllers // <<< SỬA: Namespace của bạn là QuanLySP
{
    [Area("QuanLySP")]
    [Authorize(Roles = "QuanLySP, Owner")]
    public class QuanLySPController : Controller
    {
        private readonly ISanPhamRepository _sanPhamRepo;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuanLySPController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public QuanLySPController(ISanPhamRepository sanPhamRepo,
                                  ApplicationDbContext context,
                                  ILogger<QuanLySPController> logger,
                                  IWebHostEnvironment webHostEnvironment)
        {
            _sanPhamRepo = sanPhamRepo;
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Lấy tất cả sản phẩm (Catalog)
            var sanPhams = await _sanPhamRepo.GetAllAsync();

            // 2. Lấy TỔNG tồn kho thực tế cho TẤT CẢ sản phẩm
            //    Bằng cách nhóm bảng LoTonKho theo M_SanPham và Sum(KhoiLuongConLai)
            var tonKhoDictionary = await _context.LoTonKhos
                .GroupBy(l => l.M_SanPham)
                .Select(g => new {
                    MaSanPham = g.Key,
                    TongTonKho = g.Sum(l => l.KhoiLuongConLai)
                })
                .ToDictionaryAsync(k => k.MaSanPham, v => v.TongTonKho);

            int inStockCount = 0;
            int outStockCount = 0;

            // 3. Đếm số lượng
            foreach (var sanPham in sanPhams)
            {
                // Tra cứu tồn kho của sản phẩm. Nếu không tìm thấy, tồn kho là 0.
                tonKhoDictionary.TryGetValue(sanPham.M_SanPham, out decimal currentStock);

                if (currentStock > 0)
                {
                    inStockCount++;
                }
                else
                {
                    outStockCount++;
                }
            }

            // 4. Gửi thống kê sang View
            ViewBag.TotalProducts = sanPhams.Count();
            ViewBag.InStockProducts = inStockCount;   // <-- Số sản phẩm còn hàng
            ViewBag.OutStockProducts = outStockCount; // <-- Số sản phẩm hết hàng

            return View(sanPhams);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            var sanPham = await _sanPhamRepo.GetByIdAsync(id);
            if (sanPham == null) return NotFound();
            decimal tongTonKho = await _context.LoTonKhos
    .Where(l => l.M_SanPham == id && l.KhoiLuongConLai > 0)
    .SumAsync(l => l.KhoiLuongConLai);

            ViewData["TongTonKho"] = tongTonKho; // Gửi tổng tồn kho qua ViewData

            return View(sanPham);
        }

        public IActionResult Create()
        {
            ViewData["M_LoaiSP"] = new SelectList(_context.LoaiSanPhams, "M_LoaiSP", "TenLoai");
            ViewData["M_DonViTinh"] = new SelectList(_context.DonViTinhs, "M_DonViTinh", "TenLoaiTinh");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            // <<< SỬA: XÓA "SoLuong" KHỎI [Bind] >>>
            [Bind("M_LoaiSP,M_DonViTinh,M_KhoLuuTru,TenSanPham,Gia,MoTa,TrangThai,HanSuDung")] SanPham sanPham,
            IFormFile ImageFile) // <<< THÊM THAM SỐ ImageFile VÀO ĐÂY
        {
            ModelState.Remove("M_SanPham");
            ModelState.Remove("AnhSanPham");
            ModelState.Remove("LoaiSanPham");
            ModelState.Remove("DonViTinh");
            ModelState.Remove("KhoLuuTru");
            ModelState.Remove("SoLuong"); // <<< THÊM: Xóa validation cho SoLuong

            if (ModelState.IsValid)
            {
                // --- XỬ LÝ FILE UPLOAD (ĐÃ BỎ COMMENT) ---
                string uniqueFileName = null;
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        try { Directory.CreateDirectory(uploadsFolder); }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Không thể tạo thư mục: {uploadsFolder}");
                            TempData["ErrorMessage"] = "Không thể tạo thư mục lưu ảnh.";
                            await LoadDropdownsAsync(sanPham);
                            return View(sanPham);
                        }
                    }

                    uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    try
                    {
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }
                        sanPham.AnhSanPham = "/images/products/" + uniqueFileName; // Gán đường dẫn web
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Lỗi IO khi lưu file ảnh: {filePath}");
                        TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu file ảnh.";
                        await LoadDropdownsAsync(sanPham);
                        return View(sanPham);
                    }
                }
                else
                {
                    sanPham.AnhSanPham = "/images/placeholder.png";
                }
                // --- KẾT THÚC XỬ LÝ FILE UPLOAD ---

                try
                {
                    int maxNumber = await _sanPhamRepo.GetMaxNumericIdAsync();
                    int nextNumericId = maxNumber + 1;
                    string newProductId = $"SP{nextNumericId:D3}";
                    sanPham.M_SanPham = newProductId;
                    sanPham.NgayTao = DateTime.UtcNow;

                    await _sanPhamRepo.AddAsync(sanPham);

                    _logger.LogInformation($"Đã tạo sản phẩm mới với mã: {newProductId}");
                    TempData["SuccessMessage"] = $"Thêm sản phẩm '{sanPham.TenSanPham}' (Mã: {newProductId}) thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tạo mã hoặc lưu sản phẩm qua repository.");
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tạo sản phẩm.";
                }
            }
            else
            {
                _logger.LogWarning("ModelState không hợp lệ khi tạo sản phẩm.");
            }

            await LoadDropdownsAsync(sanPham);
            return View(sanPham);
        }

        // <<< ================= VIẾT LẠI HOÀN TOÀN HÀM NÀY ================= >>>
        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file Excel hợp lệ!";
                return RedirectToAction(nameof(Index));
            }

            // Giả định: File Excel phải có cột "MaKho" để biết nhập vào kho nào.
            // Nếu không có, chúng ta phải gán cứng một kho (ví dụ: 'K001')
            const string DEFAULT_WAREHOUSE = "K001";

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage(file.OpenReadStream());
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    TempData["ErrorMessage"] = "File Excel không có dữ liệu!";
                    return RedirectToAction(nameof(Index));
                }

                var sanPhamList = new List<SanPham>();
                var loTonKhoList = new List<LoTonKho>(); // <<< THÊM: List để chứa tồn kho
                var errorMessages = new List<string>();

                // Lấy trước các DonViTinh và LoaiSanPham để tra cứu nhanh
                var allDonViTinhs = await _context.DonViTinhs.ToDictionaryAsync(d => d.M_DonViTinh, d => d.TenLoaiTinh);
                var allLoaiSanPhams = await _context.LoaiSanPhams.ToDictionaryAsync(l => l.M_LoaiSP, l => l.TenLoai);
                var allKhoHangs = await _context.KhoHangs.Select(k => k.MaKho).ToListAsync();

                using var transaction = await _context.Database.BeginTransactionAsync();

                for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                {
                    var maSP = worksheet.Cells[row, 1].Text?.Trim();
                    var maLoaiSP = worksheet.Cells[row, 2].Text?.Trim();
                    var donViText = worksheet.Cells[row, 3].Text?.Trim();
                    var ten = worksheet.Cells[row, 4].Text?.Trim();
                    var giaText = worksheet.Cells[row, 5].Text?.Trim();
                    var moTa = worksheet.Cells[row, 6].Text?.Trim();
                    var trangThai = worksheet.Cells[row, 7].Text?.Trim();
                    var ngayNhapText = worksheet.Cells[row, 8].Text?.Trim();
                    var hinhAnh = worksheet.Cells[row, 9].Text?.Trim();
                    var soLuongText = worksheet.Cells[row, 10].Text?.Trim();
                    var hanSuDungText = worksheet.Cells[row, 11].Text?.Trim();
                    // Giả sử cột 12 là MaKho, nếu không có, dùng DEFAULT_WAREHOUSE
                    var maKho = worksheet.Cells[row, 12].Text?.Trim() ?? DEFAULT_WAREHOUSE;

                    if (string.IsNullOrWhiteSpace(ten) || string.IsNullOrWhiteSpace(maSP))
                        continue;

                    // --- Validation (Giữ nguyên) ---
                    if (!allDonViTinhs.ContainsKey(donViText))
                    {
                        errorMessages.Add($"Dòng {row} bị bỏ qua: không tìm thấy đơn vị tính '{donViText}'.");
                        continue;
                    }
                    if (!allLoaiSanPhams.ContainsKey(maLoaiSP))
                    {
                        errorMessages.Add($"Dòng {row} bị bỏ qua: không tìm thấy loại sản phẩm '{maLoaiSP}'.");
                        continue;
                    }
                    if (!allKhoHangs.Contains(maKho))
                    {
                        errorMessages.Add($"Dòng {row} bị bỏ qua: không tìm thấy Mã Kho '{maKho}'. Dùng kho mặc định '{DEFAULT_WAREHOUSE}' thất bại.");
                        continue; // Bỏ qua nếu kho không hợp lệ
                    }
                    if (await _context.SanPhams.AnyAsync(s => s.M_SanPham == maSP))
                    {
                        errorMessages.Add($"Dòng {row} bị bỏ qua: Mã sản phẩm '{maSP}' đã tồn tại.");
                        continue;
                    }

                    decimal.TryParse(giaText, out decimal gia);
                    decimal.TryParse(soLuongText, out decimal soLuong); // <<< SỬA: Dùng decimal
                    DateTime.TryParse(ngayNhapText, out DateTime ngayNhap);
                    DateTime.TryParse(hanSuDungText, out DateTime hanSuDung);

                    // 🔹 Tạo sản phẩm (KHÔNG CÓ SoLuong)
                    var sanPham = new SanPham
                    {
                        M_SanPham = maSP,
                        TenSanPham = ten,
                        Gia = (long)gia,
                        MoTa = moTa,
                        TrangThai = trangThai,
                        NgayTao = ngayNhap == default ? DateTime.Now : ngayNhap,
                        AnhSanPham = hinhAnh,
                        HanSuDung = hanSuDung == default ? DateTime.Now.AddYears(1) : hanSuDung,
                        M_DonViTinh = donViText,
                        M_LoaiSP = maLoaiSP
                    };
                    sanPhamList.Add(sanPham);

                    // 🔹 TẠO LÔ TỒN KHO (LOGIC MỚI)
                    if (soLuong > 0)
                    {
                        var loTonKho = new LoTonKho
                        {
                            MaLoTonKho = $"IMP_{maSP}", // Tạo mã lô tạm
                            M_SanPham = maSP,
                            MaKho = maKho,
                            NgayNhapKho = ngayNhap == default ? DateTime.Now : ngayNhap,
                            HanSuDung = hanSuDung == default ? (DateTime?)null : hanSuDung,
                            KhoiLuongBanDau = soLuong,
                            KhoiLuongConLai = soLuong,
                            M_DonViTinh = donViText,
                            GhiChu = "Import từ Excel"
                        };
                        loTonKhoList.Add(loTonKho);
                    }
                }

                if (sanPhamList.Any())
                {
                    _context.SanPhams.AddRange(sanPhamList);
                    await _context.SaveChangesAsync(); // Lưu SanPhams trước

                    if (loTonKhoList.Any())
                    {
                        _context.LoTonKhos.AddRange(loTonKhoList);
                        await _context.SaveChangesAsync(); // Lưu LoTonKhos
                    }

                    await transaction.CommitAsync(); // Commit
                    TempData["SuccessMessage"] = $"Đã nhập {sanPhamList.Count} sản phẩm và {loTonKhoList.Count} lô tồn kho thành công!";
                }
                else
                {
                    await transaction.RollbackAsync();
                    if (!errorMessages.Any())
                        TempData["ErrorMessage"] = "File Excel không có dữ liệu hợp lệ.";
                }

                if (errorMessages.Any())
                    TempData["ErrorMessage"] = string.Join("<br/>", errorMessages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Import Excel");
                TempData["ErrorMessage"] = "Lỗi khi lưu dữ liệu: " + (ex.InnerException?.Message ?? ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        // <<< ================= VIẾT LẠI HOÀN TOÀN HÀM NÀY ================= >>>
        [HttpGet]
        public async Task<IActionResult> ExportExcel()
        {
            try
            {
                // Logic mới: Lấy Sum(Tồn kho) từ LoTonKhos
                var tonKhoQuery = _context.LoTonKhos
                    .GroupBy(l => new { l.M_SanPham, l.M_DonViTinh })
                    .Select(g => new
                    {
                        g.Key.M_SanPham,
                        g.Key.M_DonViTinh,
                        SoLuong = g.Sum(l => l.KhoiLuongConLai)
                    });

                // Join với bảng SanPhams để lấy thông tin chi tiết
                var sanPhams = await _context.SanPhams
                    .Include(sp => sp.LoaiSanPham)
                    .Include(sp => sp.DonViTinh)
                    .GroupJoin(tonKhoQuery, // Bảng SanPhams
                               sp => sp.M_SanPham, // Khóa chính của SanPham
                               tk => tk.M_SanPham, // Khóa ngoại của Tồn Kho
                               (sp, tkGroup) => new { sp, tkGroup })
                    .SelectMany(
                        x => x.tkGroup.DefaultIfEmpty(),
                        (x, tk) => new // Tạo đối tượng kết quả
                        {
                            x.sp.TenSanPham,
                            x.sp.Gia,
                            SoLuong = tk != null ? tk.SoLuong : 0, // Lấy Sum, nếu không có lô nào thì là 0
                            DonViTinh = x.sp.DonViTinh != null ? x.sp.DonViTinh.TenLoaiTinh : (tk != null ? tk.M_DonViTinh : ""),
                            LoaiSanPham = x.sp.LoaiSanPham != null ? x.sp.LoaiSanPham.TenLoai : ""
                        })
                    .OrderBy(r => r.TenSanPham)
                    .ToListAsync();


                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var package = new ExcelPackage();
                var ws = package.Workbook.Worksheets.Add("DanhSachSanPham");

                // Header
                ws.Cells["A1"].Value = "Tên sản phẩm";
                ws.Cells["B1"].Value = "Giá";
                ws.Cells["C1"].Value = "Số lượng (Tồn kho)"; // <<< SỬA
                ws.Cells["D1"].Value = "Đơn vị tính";
                ws.Cells["E1"].Value = "Loại sản phẩm";

                using (var range = ws.Cells["A1:E1"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                int row = 2;
                foreach (var sp in sanPhams)
                {
                    ws.Cells[row, 1].Value = sp.TenSanPham;
                    ws.Cells[row, 2].Value = sp.Gia;
                    ws.Cells[row, 3].Value = sp.SoLuong; // <<< SỬA: Giờ đây là TỔNG TỒN KHO
                    ws.Cells[row, 4].Value = sp.DonViTinh;
                    ws.Cells[row, 5].Value = sp.LoaiSanPham;
                    row++;
                }

                ws.Cells.AutoFitColumns();

                var excelBytes = package.GetAsByteArray();
                var fileName = $"TonKhoSanPham_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Xuất Excel");
                TempData["ErrorMessage"] = "Xuất Excel thất bại: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) { return NotFound(); }
            var sanPham = await _sanPhamRepo.GetByIdAsync(id);
            if (sanPham == null) { return NotFound(); }
            await LoadDropdownsAsync(sanPham);
            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, SanPham sanPham, IFormFile ImageFile)
        {
            if (id != sanPham.M_SanPham) return NotFound();

            var sanPhamToUpdate = await _sanPhamRepo.GetByIdAsync(id);
            if (sanPhamToUpdate == null) return NotFound();

            string oldImagePath = sanPhamToUpdate.AnhSanPham;

            ModelState.Remove("ImageFile");
            ModelState.Remove("LoaiSanPham");
            ModelState.Remove("DonViTinh");
            ModelState.Remove("KhoLuuTru");
            ModelState.Remove("SoLuong"); // <<< THÊM: Xóa validation cho SoLuong


            // <<< SỬA: XÓA "s => s.SoLuong" KHỎI DANH SÁCH CẬP NHẬT >>>
            if (await TryUpdateModelAsync<SanPham>(
                sanPhamToUpdate,
                "",
                s => s.M_LoaiSP, s => s.M_DonViTinh, s => s.TenSanPham, s => s.Gia, s => s.MoTa, s => s.TrangThai, s => s.HanSuDung
            ))
            {
                // (Phần xử lý ảnh giữ nguyên)
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    try
                    {
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }

                        sanPhamToUpdate.AnhSanPham = "/images/products/" + uniqueFileName;

                        if (!string.IsNullOrEmpty(oldImagePath) && oldImagePath != "/images/placeholder.png")
                        {
                            var oldFullPath = Path.Combine(_webHostEnvironment.WebRootPath, oldImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldFullPath))
                            {
                                try { System.IO.File.Delete(oldFullPath); } catch (Exception delEx) { _logger.LogError(delEx, $"Lỗi xóa ảnh cũ: {oldFullPath}"); }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Lỗi khi lưu file ảnh mới (Edit): {filePath}");
                        TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu file ảnh mới.";
                        await LoadDropdownsAsync(sanPhamToUpdate);
                        return View(sanPhamToUpdate);
                    }
                }

                try
                {
                    await _sanPhamRepo.UpdateAsync(sanPhamToUpdate);
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _sanPhamRepo.ExistsAsync(sanPham.M_SanPham)) return NotFound();
                    else throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật sản phẩm (Edit POST).");
                    TempData["ErrorMessage"] = "Lỗi khi cập nhật sản phẩm.";
                }
            }
            else
            {
                _logger.LogWarning("TryUpdateModelAsync thất bại khi Edit sản phẩm.");
                TempData["ErrorMessage"] = "Thông tin cập nhật không hợp lệ.";
            }

            await LoadDropdownsAsync(sanPhamToUpdate);
            return View(sanPhamToUpdate);
        }


        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var sanPham = await _sanPhamRepo.GetByIdAsync(id);
            if (sanPham == null) return NotFound();
            return View(sanPham);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            // (Logic xóa của bạn đã đúng, giữ nguyên)
            var sanPham = await _sanPhamRepo.GetByIdAsync(id);
            if (sanPham != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(sanPham.AnhSanPham) && sanPham.AnhSanPham != "/images/placeholder.png")
                    {
                        var filePath = Path.Combine(_webHostEnvironment.WebRootPath, sanPham.AnhSanPham.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            try { System.IO.File.Delete(filePath); } catch (Exception delEx) { _logger.LogError(delEx, $"Lỗi xóa file ảnh khi DeleteConfirmed: {filePath}"); }
                        }
                    }

                    await _sanPhamRepo.DeleteAsync(id);
                    TempData["SuccessMessage"] = $"Xóa sản phẩm '{sanPham.TenSanPham}' thành công!";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Lỗi khi xóa sản phẩm có mã: {id}");
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa sản phẩm. Có thể sản phẩm đang được sử dụng ở nơi khác.";
                }
            }
            else { TempData["ErrorMessage"] = "Không tìm thấy sản phẩm để xóa."; }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync(SanPham sanPham = null)
        {
            ViewData["M_LoaiSP"] = new SelectList(await _context.LoaiSanPhams.OrderBy(l => l.TenLoai).ToListAsync(), "M_LoaiSP", "TenLoai", sanPham?.M_LoaiSP);
            ViewData["M_DonViTinh"] = new SelectList(await _context.DonViTinhs.OrderBy(d => d.TenLoaiTinh).ToListAsync(), "M_DonViTinh", "TenLoaiTinh", sanPham?.M_DonViTinh);
        }
    }
}