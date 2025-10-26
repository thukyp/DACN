using DACS.Models;
using DACS.Models.ViewModels; // Namespace của ViewModels
using Microsoft.AspNetCore.Authorization; // Cho [Authorize]
using Microsoft.AspNetCore.Identity; // Cho UserManager
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DACS.Areas.QuanLyXNK.Controllers
{
    [Area("QuanLyXNK")] // Chỉ định Area
    [Authorize(Roles = "Owner,QuanLyXNK")] // Ví dụ phân quyền: Chỉ Admin hoặc người có role QuanLyXNK mới vào được
    public class QuanLyXNKController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<QuanLyXNKController> _logger; // Thêm ILogger

        public QuanLyXNKController(ApplicationDbContext context,
                                   UserManager<ApplicationUser> userManager,
                                   ILogger<QuanLyXNKController> logger) // Inject ILogger
        {
            _context = context;
            _userManager = userManager;
            _logger = logger; // Gán logger
        }

        // GET: QuanLyXNK/QuanLyXNK
        public async Task<IActionResult> Index(string? searchTerm, DateTime? dateFilter, string? statusFilter, string? collectorFilter, int page = 1)
        {
            int pageSize = 10; // Số lượng mục trên mỗi trang

            // --- Query cơ sở ---
            var query = _context.YeuCauThuGoms
                .Include(yc => yc.KhachHang)
                    .ThenInclude(kh => kh.XaPhuong) // Nối các bảng địa chỉ để lấy tên
                .Include(yc => yc.KhachHang)
                    .ThenInclude(kh => kh.QuanHuyen)
                .Include(yc => yc.KhachHang)
                    .ThenInclude(kh => kh.TinhThanhPho)
                .Include(yc => yc.QuanLy) // Nối bảng User để lấy tên người thu gom
                .Include(yc => yc.ChiTietThuGoms) // Nối chi tiết
                    .ThenInclude(ct => ct.LoaiSanPham) // Nối loại sản phẩm từ chi tiết
                .Include(yc => yc.ChiTietThuGoms)
                    .ThenInclude(ct => ct.DonViTinh) // Nối đơn vị tính từ chi tiết
                .AsQueryable(); // Bắt đầu xây dựng query

            if (!string.IsNullOrEmpty(searchTerm))
            {
                string lowerSearchTerm = searchTerm.ToLower().Trim();
                query = query.Where(yc =>
                    (yc.M_YeuCau != null && yc.M_YeuCau.ToLower().Contains(lowerSearchTerm)) || // <<< THÊM yc.M_YeuCau != null
                    (yc.KhachHang != null && yc.KhachHang.Ten_KhachHang != null && yc.KhachHang.Ten_KhachHang.ToLower().Contains(lowerSearchTerm)) ||
                    (yc.KhachHang != null && yc.KhachHang.SDT_KhachHang != null && yc.KhachHang.SDT_KhachHang.Contains(lowerSearchTerm)) // Đối với SDT, ToLower() có thể không cần thiết nếu nó chỉ chứa số.
                                                                                                                                         // Tuy nhiên, Contains trên chuỗi thường phân biệt chữ hoa/thường ở phía CSDL trừ khi collation của CSDL là case-insensitive.
                                                                                                                                         // Để đảm bảo tìm kiếm không phân biệt hoa/thường cho SDT nếu có chữ, bạn có thể thêm .ToLower() cho cả SDT_KhachHang.
                );
            }

            // --- Áp dụng Bộ lọc ---
            if (!string.IsNullOrEmpty(searchTerm))
            {
                string lowerSearchTerm = searchTerm.ToLower().Trim();
                query = query.Where(yc => yc.M_YeuCau.ToLower().Contains(lowerSearchTerm) ||
                                           (yc.KhachHang != null && yc.KhachHang.Ten_KhachHang != null && yc.KhachHang.Ten_KhachHang.ToLower().Contains(lowerSearchTerm)) ||
                                           (yc.KhachHang != null && yc.KhachHang.SDT_KhachHang != null && yc.KhachHang.SDT_KhachHang.Contains(lowerSearchTerm)));
                // Thêm tìm kiếm theo các trường khác nếu cần
            }

            if (dateFilter.HasValue)
            {
                // Lọc theo ngày sẵn sàng/ngày thu gom dự kiến (ThoiGianSanSang)
                query = query.Where(yc => yc.ThoiGianSanSang.Date == dateFilter.Value.Date);
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all") // Giả sử "all" là giá trị cho "Tất cả trạng thái"
            {
                query = query.Where(yc => yc.TrangThai == statusFilter);
            }

            if (!string.IsNullOrEmpty(collectorFilter) && collectorFilter != "all") // Giả sử "all" là giá trị cho "Tất cả người thu gom"
            {
                query = query.Where(yc => yc.M_QuanLy == collectorFilter); // Lọc theo ID người thu gom
            }

            // --- Sắp xếp ---
            query = query.OrderByDescending(yc => yc.NgayYeuCau); // Mới nhất lên đầu

            // --- Lấy Tổng số bản ghi (sau khi lọc) để phân trang ---
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // --- Phân trang ---
            var pagedData = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // --- Tính toán Thống kê ---
            var stats = await CalculateStatisticsAsync(query); // Tính trên query đã lọc hoặc _context.YeuCauThuGoms nếu muốn tổng quát

            // --- Tạo Danh sách ViewModel cho Bảng ---
            var listViewModel = pagedData.Select(yc => MapToListItemViewModel(yc)).ToList();

            // --- Chuẩn bị Options cho Dropdown Filters ---
            var statusOptions = await GetStatusOptionsAsync(statusFilter);
            var collectorOptions = await GetCollectorOptionsAsync(collectorFilter);

            var activeWarehouses = await _context.KhoHangs
                                   .Where(kh => kh.TrangThai != KhoHangTrangThai.BaoTri) // Chỉ lấy kho không bảo trì
                                   .OrderBy(kh => kh.TenKho)
                                   .Select(kh => new SelectListItem
                                   {
                                       Value = kh.MaKho,
                                       Text = $"{kh.TenKho} ({kh.MaKho})" // Hiển thị Tên (Mã)
                                   })
                                   .ToListAsync();
            // --- Tạo ViewModel chính ---
            var viewModel = new QuanLyThuGomViewModel
            {
                DanhSachYeuCau = listViewModel,
                Statistics = stats,
                SearchTerm = searchTerm,
                DateFilter = dateFilter,
                StatusFilter = statusFilter,
                CollectorFilter = collectorFilter,
                StatusOptions = statusOptions,
                CollectorOptions = collectorOptions,
                ActiveKhoHangOptions = activeWarehouses,
                PageIndex = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // --- Hàm Helper để Map YeuCauThuGom sang YeuCauListItemViewModel ---
        private YeuCauListItemViewModel MapToListItemViewModel(YeuCauThuGom yc)
        {
            var firstDetail = yc.ChiTietThuGoms?.FirstOrDefault(); // Lấy chi tiết đầu tiên để hiển thị tóm tắt
            var kh = yc.KhachHang;

            // Xây dựng địa chỉ tóm tắt
            var diaChiParts = new List<string?>();
            if (!string.IsNullOrWhiteSpace(kh?.DiaChi_DuongApThon)) diaChiParts.Add(kh.DiaChi_DuongApThon);
            if (!string.IsNullOrWhiteSpace(kh?.XaPhuong?.TenXa)) diaChiParts.Add(kh.XaPhuong.TenXa);
            if (!string.IsNullOrWhiteSpace(kh?.QuanHuyen?.TenQuan)) diaChiParts.Add(kh.QuanHuyen.TenQuan);
            if (!string.IsNullOrWhiteSpace(kh?.TinhThanhPho?.TenTinh)) diaChiParts.Add(kh.TinhThanhPho.TenTinh);
            string diaChiTomTat = string.Join(", ", diaChiParts.Where(s => !string.IsNullOrEmpty(s)));


            return new YeuCauListItemViewModel
            {
                M_YeuCau = yc.M_YeuCau,
                TenKhachHang = kh?.Ten_KhachHang,
                SdtKhachHang = kh?.SDT_KhachHang,
                DiaChiTomTat = diaChiTomTat,
                TenLoaiSanPham = firstDetail?.LoaiSanPham?.TenLoai, // Có thể null
                SoLuong = firstDetail?.SoLuong ?? 0,                      // Có thể null
                TenDonViTinh = firstDetail?.DonViTinh?.TenLoaiTinh,       // Có thể null
                NgayYeuCau = yc.NgayYeuCau,
                NgayThuGom = yc.ThoiGianSanSang != default ? yc.ThoiGianSanSang : (DateTime?)null, // Hoặc ThoiGianHoanThanh tùy logic
                TrangThai = yc.TrangThai,
                TenNguoiThuGom = yc.QuanLy?.FullName // Giả sử ApplicationUser có FullName
                                ?? yc.QuanLy?.UserName // Hoặc UserName
                                ?? "Chưa gán"
            };
        }

        // --- Hàm Helper để tính Thống kê ---
        private async Task<ThuGomStatisticsViewModel> CalculateStatisticsAsync(IQueryable<YeuCauThuGom> baseQuery)
        {
            // Clone query để không ảnh hưởng query chính hoặc tính trên toàn bộ nếu cần
            var allRequests = _context.YeuCauThuGoms; // Lấy toàn bộ yêu cầu để tính tổng quát
            var today = DateTime.UtcNow.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday); // Tính ngày đầu tuần (Thứ 2)
            if (today.DayOfWeek == DayOfWeek.Sunday) // Nếu hôm nay là CN, lùi lại 1 tuần
            {
                startOfWeek = startOfWeek.AddDays(-7);
            }

            return new ThuGomStatisticsViewModel
            {
                YeuCauMoi = await allRequests.CountAsync(yc => yc.TrangThai == "Chờ xử lý"),
                DaLenLich = await allRequests.CountAsync(yc => yc.TrangThai == "Đã lên lịch"),
                DangThucHien = await allRequests.CountAsync(yc => yc.TrangThai == "Đang thu gom"),
                HoanThanhTrongTuan = await allRequests.CountAsync(yc =>
                    (yc.TrangThai == "Hoàn thành" || yc.TrangThai == "Thu gom thành công") &&
                    yc.ThoiGianHoanThanh >= startOfWeek && yc.ThoiGianHoanThanh < startOfWeek.AddDays(7)) // Hoàn thành từ T2 đến CN tuần này
            };
        }


        // --- Hàm Helper để lấy các Tùy chọn Trạng thái ---
        private async Task<List<SelectListItem>> GetStatusOptionsAsync(string? selectedStatus)
        {
            // Lấy các trạng thái phân biệt từ DB hoặc định nghĩa cứng
            var statuses = await _context.YeuCauThuGoms
                                        .Select(yc => yc.TrangThai)
                                        .Distinct()
                                        .Where(s => !string.IsNullOrEmpty(s)) // Bỏ qua trạng thái null/rỗng
                                        .OrderBy(s => s)
                                        .ToListAsync();

            var options = new List<SelectListItem>
            {
                new SelectListItem { Value = "all", Text = "Tất cả trạng thái" }
            };

            options.AddRange(statuses.Select(s => new SelectListItem
            {
                Value = s,
                Text = s, // Có thể chuyển đổi thành tên thân thiện hơn nếu cần
                Selected = s == selectedStatus
            }));

            return options;
        }

        // --- Hàm Helper để lấy các Tùy chọn Người thu gom ---
        private async Task<List<SelectListItem>> GetCollectorOptionsAsync(string? selectedCollectorId)
        {
            // Lấy danh sách người dùng có vai trò là "Collector" hoặc "QuanLyXNK" hoặc "Admin" (tùy theo logic gán việc)
            // Ví dụ: Lấy tất cả Admin và QuanLyXNK
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var managers = await _userManager.GetUsersInRoleAsync("QuanLyXNK");
            var collectors = admins.Union(managers).DistinctBy(u => u.Id).OrderBy(u => u.UserName).ToList(); // Kết hợp và sắp xếp


            var options = new List<SelectListItem>
            {
                new SelectListItem { Value = "all", Text = "Tất cả người thu gom" }
            };

            options.AddRange(collectors.Select(u => new SelectListItem
            {
                Value = u.Id, // Giá trị là UserId
                Text = u.FullName ?? u.UserName ?? u.Email, // Hiển thị tên
                Selected = u.Id == selectedCollectorId
            }));

            // Thêm tùy chọn "Chưa gán" nếu cần lọc các yêu cầu chưa có người thu gom
            options.Add(new SelectListItem { Value = "unassigned", Text = "Chưa gán", Selected = selectedCollectorId == "unassigned" });


            return options;
        }


        // --- CÁC ACTION METHOD KHÁC (Placeholder) ---

        // GET: QuanLyXNK/QuanLyXNK/Details/YC001
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var yeuCau = await _context.YeuCauThuGoms
                .Include(yc => yc.KhachHang)
                     .ThenInclude(kh => kh.XaPhuong)
                 .Include(yc => yc.KhachHang)
                     .ThenInclude(kh => kh.QuanHuyen)
                 .Include(yc => yc.KhachHang)
                     .ThenInclude(kh => kh.TinhThanhPho)
                .Include(yc => yc.QuanLy)
                .Include(yc => yc.ChiTietThuGoms)
                    .ThenInclude(ct => ct.LoaiSanPham)
                .Include(yc => yc.ChiTietThuGoms)
                    .ThenInclude(ct => ct.DonViTinh)
                .FirstOrDefaultAsync(m => m.M_YeuCau == id);

            if (yeuCau == null) return NotFound();
            return View(yeuCau); // Tạm thời trả về Model gốc
        }

        // GET: QuanLyXNK/QuanLyXNK/Schedule/YC001
        public async Task<IActionResult> Schedule(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var yeuCau = await _context.YeuCauThuGoms
                                     .Include(yc => yc.KhachHang) // Lấy thêm thông tin KH để hiển thị context
                                     .FirstOrDefaultAsync(yc => yc.M_YeuCau == id);

            if (yeuCau == null || yeuCau.TrangThai != "Chờ xử lý")
            {
                TempData["ErrorMessage"] = "Yêu cầu không tồn tại hoặc không ở trạng thái 'Chờ xử lý'.";
                return RedirectToAction(nameof(Index));
            }

            // Tạo ViewModel đơn giản chỉ chứa thông tin cần thiết cho việc đặt lịch
            var viewModel = new ScheduleViewModel
            {
                M_YeuCau = yeuCau.M_YeuCau,
                TenKhachHang = yeuCau.KhachHang?.Ten_KhachHang, // Hiển thị context
                DiaChiTomTat = FormatAddress(yeuCau.KhachHang), // Hiển thị context (cần helper FormatAddress)
                ThoiGianSanSang = DateTime.Now.AddDays(1) // Đặt giá trị mặc định hợp lý
            };

            return View(viewModel); // Trả về view Schedule.cshtml
        }

        // POST: QuanLyXNK/QuanLyXNK/Schedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(ScheduleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Nếu model không hợp lệ, trả lại view với lỗi
                // Cần lấy lại thông tin context nếu view cần hiển thị lại
                var yeuCauContext = await _context.YeuCauThuGoms
                                    .Include(yc => yc.KhachHang)
                                    .AsNoTracking() // Chỉ đọc, không theo dõi thay đổi
                                    .FirstOrDefaultAsync(yc => yc.M_YeuCau == model.M_YeuCau);
                if (yeuCauContext != null)
                {
                    model.TenKhachHang = yeuCauContext.KhachHang?.Ten_KhachHang;
                    model.DiaChiTomTat = FormatAddress(yeuCauContext.KhachHang);
                }
                return View(model);
            }

            var yeuCau = await _context.YeuCauThuGoms.FindAsync(model.M_YeuCau);

            if (yeuCau == null || yeuCau.TrangThai != "Chờ xử lý")
            {
                TempData["ErrorMessage"] = "Yêu cầu không tồn tại hoặc đã được xử lý.";
                return RedirectToAction(nameof(Index));
            }

            // Cập nhật thời gian và trạng thái
            yeuCau.ThoiGianSanSang = model.ThoiGianSanSang;
            yeuCau.TrangThai = "Đã lên lịch";

            try
            {
                _context.Update(yeuCau);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã lên lịch thành công cho yêu cầu {yeuCau.M_YeuCau}.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                // Log lỗi và báo lỗi
                ModelState.AddModelError("", "Lỗi khi lưu lịch hẹn: " + ex.Message);
                // Lấy lại context để hiển thị
                var yeuCauContext = await _context.YeuCauThuGoms
                                    .Include(yc => yc.KhachHang)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(yc => yc.M_YeuCau == model.M_YeuCau);
                if (yeuCauContext != null)
                {
                    model.TenKhachHang = yeuCauContext.KhachHang?.Ten_KhachHang;
                    model.DiaChiTomTat = FormatAddress(yeuCauContext.KhachHang);
                }
                return View(model);
            }
        }

        private string? FormatAddress(Models.KhachHang? khachHang)
        {
            if (khachHang == null) return "N/A";
            var parts = new List<string?>();
            if (!string.IsNullOrWhiteSpace(khachHang.DiaChi_DuongApThon)) parts.Add(khachHang.DiaChi_DuongApThon);
            // Assuming navigation properties are loaded or accessible:
            // if (kh.XaPhuong != null && !string.IsNullOrWhiteSpace(kh.XaPhuong.TenXaPhuong)) parts.Add(kh.XaPhuong.TenXaPhuong);
            // if (kh.QuanHuyen != null && !string.IsNullOrWhiteSpace(kh.QuanHuyen.TenQuanHuyen)) parts.Add(kh.QuanHuyen.TenQuanHuyen);
            // if (kh.TinhThanhPho != null && !string.IsNullOrWhiteSpace(kh.TinhThanhPho.TenTinhThanhPho)) parts.Add(kh.TinhThanhPho.TenTinhThanhPho);
            if (!parts.Any()) return "Chưa rõ địa chỉ chi tiết";
            return string.Join(", ", parts.Where(s => !string.IsNullOrEmpty(s)));
        }

        public async Task<IActionResult> EditSchedule(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var yeuCau = await _context.YeuCauThuGoms
                                     .Include(yc => yc.KhachHang)
                                     .FirstOrDefaultAsync(yc => yc.M_YeuCau == id);

            // Chỉ cho phép sửa lịch khi đã lên lịch hoặc đang thu gom? Tùy logic của bạn.
            // Ví dụ: Chỉ cho sửa khi "Đã lên lịch"
            if (yeuCau == null || yeuCau.TrangThai != "Đã lên lịch")
            {
                TempData["ErrorMessage"] = "Yêu cầu không tồn tại hoặc không ở trạng thái cho phép sửa lịch.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new ScheduleViewModel
            {
                M_YeuCau = yeuCau.M_YeuCau,
                TenKhachHang = yeuCau.KhachHang?.Ten_KhachHang,
                DiaChiTomTat = FormatAddress(yeuCau.KhachHang), // Cần helper FormatAddress
                                                                // Gán giá trị ThoiGianSanSang hiện tại vào ViewModel để hiển thị trên form
                ThoiGianSanSang = yeuCau.ThoiGianSanSang
            };

            return View(viewModel); // Trả về view EditSchedule.cshtml
        }

        // POST: QuanLyXNK/QuanLyXNK/EditSchedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSchedule(ScheduleViewModel model)
        {
            // Kiểm tra ModelState trước
            if (!ModelState.IsValid)
            {
                // Lấy lại thông tin context nếu validation thất bại và cần trả về view
                var yeuCauContext = await _context.YeuCauThuGoms
                                    .Include(yc => yc.KhachHang)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(yc => yc.M_YeuCau == model.M_YeuCau);
                if (yeuCauContext != null)
                {
                    model.TenKhachHang = yeuCauContext.KhachHang?.Ten_KhachHang;
                    model.DiaChiTomTat = FormatAddress(yeuCauContext.KhachHang);
                }
                return View(model);
            }

            var yeuCau = await _context.YeuCauThuGoms.FindAsync(model.M_YeuCau);

            // Kiểm tra lại trạng thái trước khi cập nhật
            if (yeuCau == null || yeuCau.TrangThai != "Đã lên lịch") // Hoặc trạng thái khác bạn cho phép sửa
            {
                TempData["ErrorMessage"] = "Yêu cầu không tồn tại hoặc trạng thái đã thay đổi, không thể cập nhật lịch.";
                return RedirectToAction(nameof(Index));
            }

            // Chỉ cập nhật thời gian, giữ nguyên trạng thái "Đã lên lịch"
            yeuCau.ThoiGianSanSang = model.ThoiGianSanSang;

            try
            {
                _context.Update(yeuCau);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật lịch thành công cho yêu cầu {yeuCau.M_YeuCau}.";
                return RedirectToAction(nameof(Index)); // Hoặc RedirectToAction("Details", new { id = yeuCau.M_YeuCau })
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật lịch: " + ex.Message);
                // Lấy lại context để hiển thị
                var yeuCauContext = await _context.YeuCauThuGoms
                                    .Include(yc => yc.KhachHang)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(yc => yc.M_YeuCau == model.M_YeuCau);
                if (yeuCauContext != null)
                {
                    model.TenKhachHang = yeuCauContext.KhachHang?.Ten_KhachHang;
                    model.DiaChiTomTat = FormatAddress(yeuCauContext.KhachHang);
                }
                return View(model); // Trả lại view với lỗi
            }
        }

        // POST: QuanLyXNK/QuanLyXNK/AdminCancel/YC001
        [HttpPost]
        [ValidateAntiForgeryToken] // Quan trọng để chống CSRF
        public async Task<IActionResult> AdminCancel(string id)
        {
            if (id == null) return NotFound();

            var yeuCau = await _context.YeuCauThuGoms.FindAsync(id);
            if (yeuCau == null || yeuCau.TrangThai != "Chờ xử lý") // Chỉ cho phép hủy YC mới? Hoặc cả YC đã lên lịch? Tùy logic
            {
                // TempData["ErrorMessage"] = "Không thể hủy yêu cầu này.";
                return RedirectToAction(nameof(Index));
            }

            yeuCau.TrangThai = "Đã hủy"; // Hoặc "Admin hủy"
            // Có thể thêm GhiChu lý do hủy nếu cần
            // yeuCau.GhiChu = "Admin hủy do...";

            try
            {
                _context.Update(yeuCau);
                await _context.SaveChangesAsync();
                // TempData["SuccessMessage"] = $"Đã hủy yêu cầu {id}.";
            }
            catch (DbUpdateException ex)
            {
                // Log lỗi
                // TempData["ErrorMessage"] = "Lỗi khi hủy yêu cầu: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }


        // POST: QuanLyXNK/QuanLyXNK/StartCollection/YC002
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartCollection(string id)
        {
            if (id == null) return NotFound();
            var yeuCau = await _context.YeuCauThuGoms.FindAsync(id);
            if (yeuCau == null || yeuCau.TrangThai != "Đã lên lịch") // Chỉ bắt đầu khi đã lên lịch
            {
                // TempData["ErrorMessage"] = "Yêu cầu không hợp lệ để bắt đầu.";
                return RedirectToAction(nameof(Index));
            }

            yeuCau.TrangThai = "Đang thu gom";
            // Có thể cập nhật thời gian bắt đầu thực tế nếu cần

            try
            {
                _context.Update(yeuCau);
                await _context.SaveChangesAsync();
                // TempData["SuccessMessage"] = $"Yêu cầu {id} đã bắt đầu thu gom.";
            }
            catch (DbUpdateException ex) { /* Log lỗi, thông báo lỗi */ }
            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkComplete(string id, [FromQuery] string targetMaKho)
        {
            if (string.IsNullOrEmpty(id)) return NotFound("Thiếu ID yêu cầu.");
            if (string.IsNullOrEmpty(targetMaKho))
            {
                TempData["ErrorMessage"] = $"Vui lòng chọn kho đích để hoàn thành yêu cầu {id}.";
                _logger.LogWarning("MarkComplete được gọi cho YC {YeuCauId} nhưng thiếu targetMaKho.", id);
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra xem kho đích có tồn tại không (Optional but good practice)
            var khoDicExists = await _context.KhoHangs.AnyAsync(kh => kh.MaKho == targetMaKho);
            if (!khoDicExists)
            {
                TempData["ErrorMessage"] = $"Kho đích '{targetMaKho}' không tồn tại.";
                _logger.LogWarning("MarkComplete được gọi cho YC {YeuCauId} với targetMaKho không hợp lệ: {targetMaKho}.", id, targetMaKho);
                return RedirectToAction(nameof(Index));
            }

            _logger.LogInformation("Bắt đầu xử lý hoàn thành YC {YeuCauId} vào Kho {targetMaKho}.", id, targetMaKho);

            // Bắt đầu một transaction để đảm bảo tính toàn vẹn
            using var transaction = await _context.Database.BeginTransactionAsync();
            _logger.LogInformation("Bắt đầu transaction cho YC {YeuCauId}.", id);

            try
            {
                // Lấy yêu cầu và các chi tiết liên quan
                var yeuCau = await _context.YeuCauThuGoms
                                         .Include(yc => yc.ChiTietThuGoms) // *** Quan trọng: Nạp chi tiết thu gom ***
                                         .FirstOrDefaultAsync(yc => yc.M_YeuCau == id);

                // Kiểm tra trạng thái yêu cầu
                if (yeuCau == null || yeuCau.TrangThai != "Đang thu gom")
                {
                    // Không cần RollbackAsync nếu chưa có thay đổi nào được chuẩn bị
                    TempData["ErrorMessage"] = "Không thể đánh dấu hoàn thành cho yêu cầu này (không tồn tại hoặc không ở trạng thái 'Đang thu gom').";
                    _logger.LogWarning("Thất bại khi MarkComplete YC {YeuCauId}: Không tìm thấy hoặc trạng thái không hợp lệ ({TrangThai}).", id, yeuCau?.TrangThai ?? "Không tìm thấy");
                    // Rollback ngay cả khi không có thay đổi để đảm bảo transaction đóng đúng cách
                    await transaction.RollbackAsync();
                    _logger.LogInformation("Đã rollback transaction do YC {YeuCauId} không hợp lệ.", id);
                    return RedirectToAction(nameof(Index));
                }

                // 1. Cập nhật trạng thái và thời gian hoàn thành của YeuCauThuGom
                yeuCau.TrangThai = "Hoàn thành"; // Hoặc "Thu gom thành công"
                yeuCau.ThoiGianHoanThanh = DateTime.UtcNow;
                _context.Update(yeuCau); // Đánh dấu YeuCau để được cập nhật
                _logger.LogInformation("Đã chuẩn bị cập nhật trạng thái YC {YeuCauId} thành 'Hoàn thành'.", id);

                // 2. Chuẩn bị cập nhật hoặc thêm mới vào bảng TonKho cho từng ChiTietThuGom
                if (yeuCau.ChiTietThuGoms != null && yeuCau.ChiTietThuGoms.Any())
                {
                    foreach (var ct in yeuCau.ChiTietThuGoms)
                    {
                        if (ct.SoLuong <= 0 || string.IsNullOrEmpty(ct.M_LoaiSP) || string.IsNullOrEmpty(ct.M_DonViTinh))
                        {
                            _logger.LogWarning("Bỏ qua ChiTietThuGom Id {ChiTietId} của YC {YeuCauId} do thiếu thông tin (Số lượng: {SoLuong}, SP: {MaSP}, DVT: {MaDVT}).", ct.M_ChiTiet, id, ct.SoLuong, ct.M_LoaiSP, ct.M_DonViTinh);
                            continue; // Bỏ qua chi tiết không hợp lệ
                        }

                        string maSanPham = ct.M_LoaiSP;
                        long soLuongCollected = ct.SoLuong; // Should be long based on TonKho.SoLuong type
                        string maDonViTinh = ct.M_DonViTinh;


                        // Tìm bản ghi tồn kho hiện có
                        var existingTonKho = await _context.TonKhos
                            .FirstOrDefaultAsync(tk => tk.MaKho == targetMaKho
                                                    && tk.M_LoaiSP == maSanPham
                                                    && tk.M_DonViTinh == maDonViTinh);

                        if (existingTonKho != null)
                        {
                            existingTonKho.KhoiLuong += soLuongCollected;
                            _context.Update(existingTonKho); // Đánh dấu TonKho để được cập nhật
                            _logger.LogInformation("Chuẩn bị cập nhật TonKho: Kho={MaKho}, SP={MaSP}, DVT={MaDVT}. Số lượng +{SoLuong}. Tổng mới: {TongSoLuong}", targetMaKho, maSanPham, maDonViTinh, soLuongCollected, existingTonKho.KhoiLuong);
                        }
                        else
                        {
                            var newTonKho = new TonKho
                            {
                                MaKho = targetMaKho,
                                M_LoaiSP = maSanPham,
                                KhoiLuong = soLuongCollected,
                                M_DonViTinh = maDonViTinh, 
                                 };
                            _context.Add(newTonKho); // Đánh dấu TonKho mới để được thêm
                            _logger.LogInformation("Chuẩn bị thêm mới TonKho: Kho={MaKho}, SP={MaSP}, DVT={MaDVT}. Số lượng: {SoLuong}", targetMaKho, maSanPham, maDonViTinh, soLuongCollected);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("YC {YeuCauId} không có ChiTietThuGom hợp lệ để cập nhật TonKho.", id);
                }

                // 3. Lưu tất cả các thay đổi đã chuẩn bị vào database trong transaction
                await _context.SaveChangesAsync(); // *** Chỉ gọi SaveChangesAsync một lần ***
                _logger.LogInformation("Đã lưu thành công các thay đổi vào DB cho YC {YeuCauId}.", id);


                // 4. Nếu mọi thứ thành công, commit transaction
                await transaction.CommitAsync();
                _logger.LogInformation("Đã commit transaction thành công cho YC {YeuCauId}.", id);
                TempData["SuccessMessage"] = $"Yêu cầu {id} đã hoàn thành và cập nhật tồn kho vào kho {targetMaKho}.";

            }
            catch (Exception ex) // Bắt DbUpdateException hoặc Exception chung
            {
                // Nếu có lỗi trong quá trình chuẩn bị hoặc SaveChangesAsync, rollback transaction
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Lỗi trong quá trình MarkComplete và cập nhật TonKho cho YC {YeuCauId}. Transaction đã được rollback.", id);
                // **Quan trọng: Log cả ex.InnerException nếu có**
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner Exception details for YC {YeuCauId}:", id);
                }
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật trạng thái và tồn kho: " + ex.Message + (ex.InnerException != null ? " Inner Error: " + ex.InnerException.Message : "");
            }

            return RedirectToAction(nameof(Index));
        }



        // POST: QuanLyXNK/QuanLyXNK/MarkFailed/YC003
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkFailed(string id, string? failureReason) // Thêm lý do thất bại nếu cần
        {
            if (id == null) return NotFound();
            var yeuCau = await _context.YeuCauThuGoms.FindAsync(id);
            // Cho phép đánh dấu thất bại khi Đang thu gom hoặc Đã lên lịch?
            if (yeuCau == null || (yeuCau.TrangThai != "Đang thu gom" && yeuCau.TrangThai != "Đã lên lịch"))
            {
                // TempData["ErrorMessage"] = "Không thể đánh dấu thất bại.";
                return RedirectToAction(nameof(Index));
            }

            yeuCau.TrangThai = "Thu gom thất bại";
            yeuCau.ThoiGianHoanThanh = DateTime.UtcNow; // Vẫn ghi nhận thời điểm kết thúc (dù là thất bại)
            if (!string.IsNullOrEmpty(failureReason))
            {
                yeuCau.GhiChu = $"Thất bại: {failureReason}"; // Ghi lại lý do
            }

            try
            {
                _context.Update(yeuCau);
                await _context.SaveChangesAsync();
                // TempData["WarningMessage"] = $"Đã ghi nhận yêu cầu {id} thu gom thất bại.";
                // TODO: Có thể cần các bước xử lý tiếp theo cho YC thất bại
            }
            catch (DbUpdateException ex) { /* Log lỗi, thông báo lỗi */ }
            return RedirectToAction(nameof(Index));
        }

        // GET: QuanLyXNK/QuanLyXNK/CreateManualEntry
        public IActionResult CreateManualEntry()
        {
            // TODO: Chuẩn bị dữ liệu cho form tạo mới (ví dụ: danh sách khách hàng, loại SP...)
            // return View(new CreateManualEntryViewModel());
            return View(); // Tạo View CreateManualEntry.cshtml
        }
    }
}