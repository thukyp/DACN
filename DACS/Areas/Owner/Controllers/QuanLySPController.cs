using DACS.Models;
using DACS.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DACS.Areas.Owner.Controllers
{
    [Area("Owner")]
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

            ViewData["TongTonKho"] = tongTonKho;
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
                // --- BẮT ĐẦU XỬ LÝ FILE UPLOAD (BỎ COMMENT) ---
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
                        sanPham.AnhSanPham = "/images/products/" + uniqueFileName;
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
                    // (Code xóa file ảnh nếu DB lỗi... đã được comment)
                }
            }
            else
            {
                _logger.LogWarning("ModelState không hợp lệ khi tạo sản phẩm.");
                TempData["ErrorMessage"] = "Thông tin sản phẩm không hợp lệ. Vui lòng kiểm tra lại.";
            }

            await LoadDropdownsAsync(sanPham);
            return View(sanPham);
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

            if (await TryUpdateModelAsync<SanPham>(
                sanPhamToUpdate,
                "",
                // <<< SỬA: XÓA "s => s.SoLuong" KHỎI DANH SÁCH CẬP NHẬT >>>
                s => s.M_LoaiSP, s => s.M_DonViTinh, s => s.TenSanPham, s => s.Gia, s => s.MoTa, s => s.TrangThai, s => s.HanSuDung
            ))
            {
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