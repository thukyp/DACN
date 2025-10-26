using DACS.Models;
using DACS.Repositories; // Using repository namespace
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting; // <<< THÊM USING NÀY
using Microsoft.AspNetCore.Http;  // <<< THÊM USING NÀY (cho IFormFile)
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // Vẫn cần cho SelectList
using Microsoft.Extensions.Logging;
using System;                      // <<< THÊM USING NÀY (cho Guid, Exception)
using System.IO;                   // <<< THÊM USING NÀY (cho Path, File, FileStream)
using System.Linq;
using System.Threading.Tasks;

namespace DACS.Areas.QuanLySP.Controllers
{
    [Area("QuanLySP")]
    [Authorize(Roles = "QuanLySP, Owner")]
    public class QuanLySPController : Controller
    {
        private readonly ISanPhamRepository _sanPhamRepo;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<QuanLySPController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment; // <<< THÊM ĐỂ LẤY PATH WWWROOT

        // Cập nhật Constructor để nhận IWebHostEnvironment
        public QuanLySPController(ISanPhamRepository sanPhamRepo,
                                 ApplicationDbContext context,
                                 ILogger<QuanLySPController> logger,
                                 IWebHostEnvironment webHostEnvironment) // <<< THÊM THAM SỐ
        {
            _sanPhamRepo = sanPhamRepo;
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment; // <<< GÁN GIÁ TRỊ
        }

        // --- Index, Details, Create (GET) giữ nguyên ---
        public async Task<IActionResult> Index()
        {
            var sanPhams = await _sanPhamRepo.GetAllAsync();
            ViewBag.TotalProducts = sanPhams.Count();
            ViewBag.InStockProducts = sanPhams.Count(p => p.SoLuong > 0);
            ViewBag.OutStockProducts = sanPhams.Count(p => p.SoLuong <= 0);
            return View(sanPhams);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            var sanPham = await _sanPhamRepo.GetByIdAsync(id);
            if (sanPham == null) return NotFound();
            return View(sanPham);
        }

        public IActionResult Create()
        {
            ViewData["M_LoaiSP"] = new SelectList(_context.LoaiSanPhams, "M_LoaiSP", "TenLoai");
            ViewData["M_DonViTinh"] = new SelectList(_context.DonViTinhs, "M_DonViTinh", "TenLoaiTinh");
            return View();
        }
        // --- Hết phần giữ nguyên ---


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("M_LoaiSP,M_DonViTinh,M_KhoLuuTru,TenSanPham,Gia,MoTa,TrangThai,SoLuong,HanSuDung")] SanPham sanPham)
            //IFormFile ImageFile) // Tên tham số khớp với name của input file
        {
            ModelState.Remove("M_SanPham"); // Bỏ qua validation cho Mã SP sẽ tự tạo
            ModelState.Remove("AnhSanPham"); // Bỏ qua validation cho Path ảnh sẽ tự gán
            // Bỏ validation cho navigation properties nếu chúng gây lỗi ModelState
            ModelState.Remove("LoaiSanPham");
            ModelState.Remove("DonViTinh");
            ModelState.Remove("KhoLuuTru");


            if (ModelState.IsValid)
            {
                // --- BẮT ĐẦU XỬ LÝ FILE UPLOAD ---
                //string uniqueFileName = null;
                //if (ImageFile != null && ImageFile.Length > 0)
                //{
                //    // Xác định thư mục lưu ảnh (ví dụ: wwwroot/images/products)
                //    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

                //    // Tạo thư mục nếu chưa có
                //    if (!Directory.Exists(uploadsFolder))
                //    {
                //        try
                //        {
                //            Directory.CreateDirectory(uploadsFolder);
                //        }
                //        catch (Exception ex)
                //        {
                //            _logger.LogError(ex, $"Không thể tạo thư mục: {uploadsFolder}");
                //            TempData["ErrorMessage"] = "Không thể tạo thư mục lưu ảnh.";
                //            // Load lại dropdown và trả về view lỗi
                //            await LoadDropdownsAsync(sanPham);
                //            return View(sanPham);
                //        }
                //    }

                //    // Tạo tên file duy nhất để tránh trùng lặp
                //    uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                //    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                //    // Lưu file ảnh vào thư mục
                //    try
                //    {
                //        using (var fileStream = new FileStream(filePath, FileMode.Create))
                //        {
                //            await ImageFile.CopyToAsync(fileStream);
                //        }
                //        _logger.LogInformation($"Đã lưu file ảnh: {filePath}");
                //        // Gán đường dẫn tương đối (dùng cho web) vào model
                //        sanPham.AnhSanPham = "/images/products/" + uniqueFileName;
                //    }
                //    catch (IOException ioEx)
                //    {
                //        _logger.LogError(ioEx, $"Lỗi IO khi lưu file ảnh: {filePath}");
                //        TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu file ảnh.";
                //        // Load lại dropdown và trả về view lỗi
                //        await LoadDropdownsAsync(sanPham);
                //        return View(sanPham);
                //    }
                //    catch (Exception ex)
                //    {
                //        _logger.LogError(ex, $"Lỗi không xác định khi lưu file ảnh: {filePath}");
                //        TempData["ErrorMessage"] = "Có lỗi không xác định khi lưu file ảnh.";
                //        // Load lại dropdown và trả về view lỗi
                //        await LoadDropdownsAsync(sanPham);
                //        return View(sanPham);
                //    }
                //}
                //else
                //{
                //    // Không có file nào được upload, gán ảnh mặc định hoặc null
                //    sanPham.AnhSanPham = "/images/placeholder.png"; // Hoặc null nếu cho phép
                //    _logger.LogWarning("Không có file ảnh nào được upload.");
                //}
                // --- KẾT THÚC XỬ LÝ FILE UPLOAD ---


                // Tạo mã SP
                try
                {
                    int maxNumber = await _sanPhamRepo.GetMaxNumericIdAsync();
                    int nextNumericId = maxNumber + 1;
                    string newProductId = $"SP{nextNumericId:D3}";
                    sanPham.M_SanPham = newProductId;
                    sanPham.NgayTao = DateTime.UtcNow;

                    // Lưu sản phẩm vào DB qua Repository
                    await _sanPhamRepo.AddAsync(sanPham);

                    _logger.LogInformation($"Đã tạo sản phẩm mới với mã: {newProductId}");
                    TempData["SuccessMessage"] = $"Thêm sản phẩm '{sanPham.TenSanPham}' (Mã: {newProductId}) thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tạo mã hoặc lưu sản phẩm qua repository.");
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tạo sản phẩm.";
                    // Cân nhắc xóa file ảnh đã lưu nếu việc lưu vào DB thất bại
                    //if (!string.IsNullOrEmpty(uniqueFileName))
                    //{
                    //    var createdFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products", uniqueFileName);
                    //    if (System.IO.File.Exists(createdFilePath))
                    //    {
                    //        try { System.IO.File.Delete(createdFilePath); } catch (Exception delEx) { _logger.LogError(delEx, $"Lỗi khi xóa file ảnh đã tạo: {createdFilePath}"); }
                    //    }
                    //}
                }
            }
            else // ModelState không hợp lệ
            {
                _logger.LogWarning("ModelState không hợp lệ khi tạo sản phẩm.");
                TempData["ErrorMessage"] = "Thông tin sản phẩm không hợp lệ. Vui lòng kiểm tra lại.";
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    foreach (var error in modelStateVal.Errors)
                    {
                        _logger.LogWarning($"Key: {modelStateKey}, Error: {error.ErrorMessage}");
                    }
                }
            }

            // Load lại dropdown nếu thất bại
            await LoadDropdownsAsync(sanPham);
            return View(sanPham);
        }

        // --- Edit GET giữ nguyên ---
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) { return NotFound(); }
            var sanPham = await _sanPhamRepo.GetByIdAsync(id);
            if (sanPham == null) { return NotFound(); }
            await LoadDropdownsAsync(sanPham); // Load dropdown cho Edit view
            return View(sanPham);
        }


        // Edit POST (Bổ sung xử lý ảnh tương tự Create)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, SanPham sanPham, IFormFile ImageFile) // Dùng TryUpdateModel sẽ tốt hơn
        {
            if (id != sanPham.M_SanPham) return NotFound();

            // Lấy bản ghi gốc từ DB
            var sanPhamToUpdate = await _sanPhamRepo.GetByIdAsync(id);
            if (sanPhamToUpdate == null) return NotFound();

            // Giữ lại đường dẫn ảnh cũ để có thể xóa nếu upload ảnh mới
            string oldImagePath = sanPhamToUpdate.AnhSanPham;

            // Bỏ validation nếu cần
            ModelState.Remove("ImageFile"); // Tham số IFormFile không cần validate trực tiếp
            ModelState.Remove("LoaiSanPham");
            ModelState.Remove("DonViTinh");
            ModelState.Remove("KhoLuuTru");


            // Cập nhật các thuộc tính từ form vào đối tượng lấy từ DB
            if (await TryUpdateModelAsync<SanPham>(
                sanPhamToUpdate,
                "", // Prefix
                 s => s.TenSanPham, s => s.Gia, s => s.MoTa, s => s.TrangThai, s => s.SoLuong, s => s.HanSuDung
            // Không cập nhật M_SanPham, NgayTao, AnhSanPham ở đây
            ))
            {
                // --- XỬ LÝ UPLOAD FILE ẢNH MỚI (NẾU CÓ) ---
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    try
                    {
                        // Tạo thư mục nếu chưa có
                        if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                        // Lưu file mới
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(fileStream);
                        }
                        _logger.LogInformation($"Đã lưu file ảnh mới cho Edit: {filePath}");

                        // Cập nhật đường dẫn ảnh mới vào đối tượng
                        sanPhamToUpdate.AnhSanPham = "/images/products/" + uniqueFileName;

                        // Xóa file ảnh cũ (nếu có và khác ảnh placeholder)
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
                // else: Không có file mới được upload, giữ nguyên sanPhamToUpdate.AnhSanPham (không làm gì cả)
                // --- KẾT THÚC XỬ LÝ FILE ẢNH MỚI ---


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
                // Log lỗi ModelState chi tiết
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    foreach (var error in modelStateVal.Errors)
                    {
                        _logger.LogWarning($"Key: {modelStateKey}, Error: {error.ErrorMessage}");
                    }
                }
            }


            // Nếu lỗi
            await LoadDropdownsAsync(sanPhamToUpdate); // Load lại dropdown với dữ liệu hiện tại
            return View(sanPhamToUpdate);
        }


        // --- Delete GET, Delete POST giữ nguyên ---
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
                    // Cân nhắc xóa file ảnh liên quan trước khi xóa record DB
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


        // Hàm helper để load dropdown, tránh lặp code
        private async Task LoadDropdownsAsync(SanPham sanPham = null)
        {
            ViewData["M_LoaiSP"] = new SelectList(await _context.LoaiSanPhams.OrderBy(l => l.TenLoai).ToListAsync(), "M_LoaiSP", "TenLoai", sanPham?.M_LoaiSP);
            ViewData["M_DonViTinh"] = new SelectList(await _context.DonViTinhs.OrderBy(d => d.TenLoaiTinh).ToListAsync(), "M_DonViTinh", "TenLoaiTinh", sanPham?.M_DonViTinh);
        }



    }
}