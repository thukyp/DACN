using DACS.Areas.KhachHang.Controllers;
using DACS.Models;
using DACS.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DACS.Services;

namespace DACS.Controllers
{
    public class ThuGomController : Controller
    {
        private readonly ILogger<ThuGomController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public ThuGomController(
            IConfiguration configuration,
            ILogger<ThuGomController> logger,
            ApplicationDbContext dbContext,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _configuration = configuration;
            _logger = logger;
            _context = dbContext;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _emailService = emailService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ThuGomViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Dữ liệu gửi lên không hợp lệ (ModelState invalid).");
                // <<< SỬA: Gọi hàm load đầy đủ để giữ lại lựa chọn dropdown
                await LoadDropdownDataForThuGom(model);
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "Không thể xác định người dùng. Vui lòng đăng nhập lại.");
                // <<< SỬA: Gọi hàm load đầy đủ
                await LoadDropdownDataForThuGom(model);
                return View(model);
            }
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(kh => kh.UserId == userId);
            if (khachHang == null)
            {
                _logger.LogError($"Không tìm thấy KhachHang cho UserId: {userId}");
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng liên kết. Vui lòng cập nhật hồ sơ.";
                return RedirectToAction("Index", "Home");
            }

            var isValidProductType = await _context.LoaiSanPhams
                                        .AnyAsync(p => p.M_LoaiSP == model.M_LoaiSP);
            if (!isValidProductType)
            {
                ModelState.AddModelError(nameof(model.M_LoaiSP), "Loại sản phẩm được chọn không hợp lệ hoặc không tồn tại.");
                _logger.LogWarning($"Khách hàng {khachHang?.M_KhachHang} đã cố gửi Loại SP không hợp lệ: {model.M_LoaiSP}");
                // <<< SỬA: Gọi hàm load đầy đủ
                await LoadDropdownDataForThuGom(model);
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            YeuCauThuGom yeuCau = null;
            try
            {
                // 1. Tạo YeuCauThuGom 
                yeuCau = new YeuCauThuGom
                {
                    M_YeuCau = GenerateRequestCode(),
                    M_KhachHang = khachHang.M_KhachHang,
                    NgayYeuCau = DateTime.UtcNow,
                    ThoiGianSanSang = model.PickupReadyTime.Value,
                    GhiChu = model.SupplierNotes,
                    TrangThai = "Chờ xử lý",
                    MaTinh = model.SupplierProvince,
                    MaQuan = model.SupplierDistrict,
                    MaXa = model.SupplierWard,
                    DiaChi_DuongApThon = model.SupplierStreet
                };

                // 2. Xử lý ảnh 
                List<string> savedImagePaths = new List<string>();
                if (model.ByproductImages != null && model.ByproductImages.Count > 0)
                {
                    string requestUploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "thugom", yeuCau.M_YeuCau);
                    Directory.CreateDirectory(requestUploadFolder);

                    foreach (var file in model.ByproductImages)
                    {
                        if (file.Length > 0 && file.ContentType.StartsWith("image/"))
                        {
                            try
                            {
                                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                                string filePath = Path.Combine(requestUploadFolder, uniqueFileName);

                                using (var fileStream = new FileStream(filePath, FileMode.Create))
                                {
                                    await file.CopyToAsync(fileStream);
                                }
                                string relativePath = Path.Combine("/uploads/thugom", yeuCau.M_YeuCau, uniqueFileName).Replace("\\", "/");
                                savedImagePaths.Add(relativePath);
                            }
                            catch (Exception imgEx)
                            {
                                _logger.LogError(imgEx, $"Lỗi khi xử lý file ảnh '{file.FileName}' cho yêu cầu {yeuCau.M_YeuCau}.");
                                ModelState.AddModelError("ByproductImages", $"Lỗi khi xử lý file '{file.FileName}'.");
                            }
                        }
                    }

                    if (!ModelState.IsValid)
                    {
                        await transaction.RollbackAsync();
                        // <<< SỬA: Gọi hàm load đầy đủ
                        await LoadDropdownDataForThuGom(model);
                        return View(model);
                    }
                }

                // 3. Tạo ChiTietThuGom 
                var chiTiet = new ChiTietThuGom
                {
                    M_ChiTiet = Guid.NewGuid().ToString("N").Substring(0, 10),
                    M_YeuCau = yeuCau.M_YeuCau,
                    M_LoaiSP = model.M_LoaiSP,       // <<< SỬA
                    M_SanPham = model.M_SanPham,    // <<< SỬA: Lấy từ model
                    M_DonViTinh = model.ByproductUnit,
                    SoLuong = (int)model.ByproductQuantity.Value,
                    MoTa = model.ByproductDescription,
                    GiaTriMongMuon = model.ByproductValue,
                    DacTinh_CongKenh = model.CharBulky,
                    DacTinh_AmUot = model.CharWet,
                    DacTinh_Kho = model.CharDry,
                    DacTinh_DoAmCao = model.CharMoisture,
                    DacTinh_TapChat = model.CharImpure,
                    DacTinh_DaXuLy = model.CharProcessed,
                    DanhSachHinhAnh = savedImagePaths.Any() ? string.Join(";", savedImagePaths) : null,
                    TrangThaiXuLy = "MoiYeuCau",
                    MaLoTonKho = null
                };

                yeuCau.ChiTietThuGoms = new List<ChiTietThuGom> { chiTiet };

                // 4. Lưu vào Database
                _context.YeuCauThuGoms.Add(yeuCau);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // <<< ============ GỬI EMAIL SAU KHI ĐẶT THU GOM THÀNH CÔNG ============ >>>
                try
                {
                    var loaiSP = await _context.LoaiSanPhams.FindAsync(model.M_LoaiSP);
                    var subject = $"Xác nhận yêu cầu thu gom #{yeuCau.M_YeuCau}";
                    var body = $@"
                        <h1>Cảm ơn bạn đã gửi yêu cầu thu gom!</h1>
                        <p>Chào {khachHang.Ten_KhachHang},</p>
                        <p>Yêu cầu thu gom <strong>#{yeuCau.M_YeuCau}</strong> của bạn đã được tiếp nhận.</p>
                        <p><strong>Loại sản phẩm:</strong> {loaiSP?.TenLoai ?? model.M_LoaiSP}</p>
                        <p><strong>Thời gian sẵn sàng:</strong> {model.PickupReadyTime.Value.ToString("g")}</p>
                        <p><strong>Địa chỉ thu gom:</strong> {model.SupplierStreet}, {model.SupplierWard}, {model.SupplierDistrict}, {model.SupplierPhone}</p>
                        <p>Chúng tôi sẽ liên hệ với bạn sớm nhất để sắp xếp.</p>
                        <p>Trân trọng,</p>
                        <p>Đội ngũ [Tên Cửa Hàng]</p>";
                        
                    // Dùng user.Email để gửi
                    await _emailService.SendEmailAsync(khachHang.Email_KhachHang, subject, body); 
                }
                catch (Exception emailEx)
                {
                    // Ghi log lỗi gửi mail nhưng không làm ảnh hưởng đến kết quả
                    _logger.LogError(emailEx, "Lỗi khi gửi email xác nhận thu gom {YeuCauId}", yeuCau?.M_YeuCau);
                }
                // <<< ================== KẾT THÚC GỬI EMAIL ================== >>>

                _logger.LogInformation($"Đã tạo yêu cầu thu gom {yeuCau.M_YeuCau} bởi khách hàng {khachHang.M_KhachHang}");
                TempData["SuccessMessage"] = $"Yêu cầu thu gom ({yeuCau.M_YeuCau}) của bạn đã được gửi thành công!";
                return RedirectToAction(nameof(KhachHangController.LichSuYeuCauThuGom), "KhachHang", new { area = "KhachHang" });
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                _logger.LogError(dbEx, $"Lỗi DbUpdateException khi lưu YC {yeuCau?.M_YeuCau}");
                ModelState.AddModelError("", "Lỗi Database khi lưu: " + dbEx.InnerException?.Message);
                // <<< SỬA: Gọi hàm load đầy đủ
                await LoadDropdownDataForThuGom(model);
                return View(model);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Lỗi không xác định khi xử lý YC {yeuCau?.M_YeuCau}");
                ModelState.AddModelError("", "Đã xảy ra lỗi không mong muốn khi tạo yêu cầu.");
                // <<< SỬA: Gọi hàm load đầy đủ
                await LoadDropdownDataForThuGom(model);
                return View(model);
            }
        }
        [HttpGet]
        public async Task<JsonResult> GetSanPhamsByLoai(string maLoaiSP)
        {
            if (string.IsNullOrEmpty(maLoaiSP))
            {
                return Json(new List<object>());
            }
            try
            {
                var sanPhams = await _context.SanPhams
                                    .Where(sp => sp.M_LoaiSP == maLoaiSP && sp.TrangThai == "Còn hàng")
                                    .OrderBy(sp => sp.TenSanPham)
                                    .Select(sp => new { id = sp.M_SanPham, name = sp.TenSanPham })
                                    .ToListAsync();
                return Json(sanPhams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy Sản Phẩm cho Loại SP ID: {MaLoaiSP}", maLoaiSP);
                return Json(new List<object>());
            }
        }
        private string GenerateRequestCode()
        {
            string prefix = "YC" + DateTime.Now.ToString("yyMMdd");
            var todayCodes = _context.YeuCauThuGoms
                .Where(y => y.M_YeuCau.StartsWith(prefix))
                .OrderByDescending(y => y.M_YeuCau)
                .Select(y => y.M_YeuCau)
                .FirstOrDefault();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(todayCodes))
            {
                string numberPart = todayCodes.Substring(prefix.Length);
                if (int.TryParse(numberPart, out int parsed))
                {
                    nextNumber = parsed + 1;
                }
            }
            return prefix + nextNumber.ToString("D2");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation($"GET Index - Initial Load");
            var model = new ThuGomViewModel();
            // <<< SỬA: Chỗ này gọi hàm "Initial" là ĐÚNG, vì đây là lần tải đầu tiên
            await LoadDropdownDataForThuGomInitial(model);
            return View(model);
        }

        private async Task LoadDropdownDataForThuGomInitial(ThuGomViewModel model)
        {
            if (model == null || _context == null) return;
            try
            {
                var loaiSpList = await _context.LoaiSanPhams.OrderBy(lsp => lsp.TenLoai).ToListAsync();
                model.LoaiSanPhamOptions = new SelectList(loaiSpList, "M_LoaiSP", "TenLoai");

                var donViTinhList = await _context.DonViTinhs.OrderBy(dvt => dvt.TenLoaiTinh).ToListAsync();
                model.DonViTinhOptions = new SelectList(donViTinhList, "M_DonViTinh", "TenLoaiTinh");

                var provinces = await _context.TinhThanhPhos.OrderBy(p => p.TenTinh).ToListAsync();
                model.ProvinceOptions = new SelectList(provinces, "MaTinh", "TenTinh");
                model.SanPhamOptions = Enumerable.Empty<SelectListItem>();
                model.DistrictOptions = Enumerable.Empty<SelectListItem>();
                model.WardOptions = Enumerable.Empty<SelectListItem>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading initial dropdown data for ThuGom");
                model.LoaiSanPhamOptions ??= Enumerable.Empty<SelectListItem>();
                model.SanPhamOptions ??= Enumerable.Empty<SelectListItem>();
                model.DonViTinhOptions ??= Enumerable.Empty<SelectListItem>();
                model.ProvinceOptions ??= Enumerable.Empty<SelectListItem>();
                model.DistrictOptions ??= Enumerable.Empty<SelectListItem>();
                model.WardOptions ??= Enumerable.Empty<SelectListItem>();
            }
        }

        // Hàm này dùng khi POST bị lỗi, để tải lại TẤT CẢ dropdown
        private async Task LoadDropdownDataForThuGom(ThuGomViewModel model)
        {
            _logger.LogInformation("--- Entering LoadDropdownDataForThuGom (Full) ---");
            if (model == null || _context == null)
            {
                _logger.LogError("Model or DbContext is NULL in LoadDropdownDataForThuGom.");
                return;
            }

            try
            {
                // 1. Load Loại Sản Phẩm
                var loaiSpList = await _context.LoaiSanPhams.OrderBy(lsp => lsp.TenLoai).ToListAsync();
                model.LoaiSanPhamOptions = new SelectList(loaiSpList, "M_LoaiSP", "TenLoai", model.M_LoaiSP);

                // 2. Load Đơn vị tính
                var donViTinhList = await _context.DonViTinhs.OrderBy(dvt => dvt.TenLoaiTinh).ToListAsync();
                model.DonViTinhOptions = new SelectList(donViTinhList, "M_DonViTinh", "TenLoaiTinh", model.ByproductUnit);

                // 3. Load Tỉnh/Thành phố
                var provinces = await _context.TinhThanhPhos.OrderBy(p => p.TenTinh).ToListAsync();
                model.ProvinceOptions = new SelectList(provinces, "MaTinh", "TenTinh", model.SupplierProvince);

                // 4. Load Quận/Huyện (dựa trên Tỉnh đã chọn)
                var districts = Enumerable.Empty<QuanHuyen>();
                if (!string.IsNullOrEmpty(model.SupplierProvince))
                {
                    districts = await _context.QuanHuyens
                                    .Where(q => q.MaTinh == model.SupplierProvince)
                                    .OrderBy(q => q.TenQuan).ToListAsync();
                }
                model.DistrictOptions = new SelectList(districts, "MaQuan", "TenQuan", model.SupplierDistrict);

                var sanPhams = Enumerable.Empty<SanPham>();
                if (!string.IsNullOrEmpty(model.M_LoaiSP)) // Chỉ load khi có Loại SP
                {
                    sanPhams = await _context.SanPhams
                                    .Where(sp => sp.M_LoaiSP == model.M_LoaiSP)
                                    .OrderBy(sp => sp.TenSanPham).ToListAsync();
                }
                model.SanPhamOptions = new SelectList(sanPhams, "M_SanPham", "TenSanPham", model.M_SanPham);

                // 5. Load Xã/Phường (dựa trên Huyện đã chọn)
                var wards = Enumerable.Empty<XaPhuong>();
                if (!string.IsNullOrEmpty(model.SupplierDistrict))
                {
                    wards = await _context.XaPhuongs
                                    .Where(x => x.MaQuan == model.SupplierDistrict)
                                    .OrderBy(x => x.TenXa).ToListAsync();
                }
                model.WardOptions = new SelectList(wards, "MaXa", "TenXa", model.SupplierWard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading full dropdown data for ThuGom");
                // Gán rỗng khi lỗi
                model.ProvinceOptions ??= Enumerable.Empty<SelectListItem>();
                model.DistrictOptions ??= Enumerable.Empty<SelectListItem>();
                model.SanPhamOptions ??= Enumerable.Empty<SelectListItem>(); // <<< THÊM MỚI
                model.WardOptions ??= Enumerable.Empty<SelectListItem>();
                model.LoaiSanPhamOptions ??= Enumerable.Empty<SelectListItem>();
                model.DonViTinhOptions ??= Enumerable.Empty<SelectListItem>();
            }
        }

        // <<< SỬA: Đổi tên ...ForHome thành ...
        [HttpGet]
        public async Task<JsonResult> GetDistricts(string provinceId)
        {
            if (string.IsNullOrEmpty(provinceId))
            {
                return Json(new List<object>());
            }
            try
            {
                var districts = await _context.QuanHuyens
                                    .Where(q => q.MaTinh == provinceId)
                                    .OrderBy(q => q.TenQuan)
                                    .Select(q => new { id = q.MaQuan, name = q.TenQuan })
                                    .ToListAsync();
                return Json(districts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy Quận/Huyện cho Tỉnh ID: {provinceId}");
                return Json(new List<object>());
            }
        }

        // <<< SỬA: Đổi tên ...ForHome thành ...
        [HttpGet]
        public async Task<JsonResult> GetWards(string districtId)
        {
            if (string.IsNullOrEmpty(districtId))
            {
                return Json(new List<object>());
            }
            try
            {
                var wards = await _context.XaPhuongs
                                    .Where(w => w.MaQuan == districtId)
                                    .OrderBy(w => w.TenXa)
                                    .Select(w => new { id = w.MaXa, name = w.TenXa })
                                    .ToListAsync();
                return Json(wards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi khi lấy Xã/Phường cho Quận ID: {districtId}");
                return Json(new List<object>());
            }
        }
    }
}