using DACS.Models;
using DACS.Repositories;
using Microsoft.AspNetCore.Identity; // <-- thêm
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DACS.Controllers
{
    public class DS_SPController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISanPhamRepository _sanphamRepository;
        private readonly UserManager<ApplicationUser> _userManager; // <-- thêm

        public DS_SPController(ApplicationDbContext context, ISanPhamRepository sanphamRepository, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _sanphamRepository = sanphamRepository;
            _userManager = userManager; // <-- thêm
        }

        public IActionResult TimKiem(string keyword)
        {
            var query = _context.SanPhams
                .Include(sp => sp.LoaiSanPham)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(sp => sp.TenSanPham.Contains(keyword));
            }

            var ketQua = query.ToList();
            return View("Index", ketQua);
        }

        [HttpGet]
        public IActionResult Suggest(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                return Json(new List<object>());

            var result = _context.SanPhams
                .Where(sp => sp.TenSanPham.Contains(keyword))
                .Select(sp => new
                {
                    m_SanPham = sp.M_SanPham,
                    tenSanPham = sp.TenSanPham
                })
                .Take(5)
                .ToList();

            return Json(result);
        }

        public async Task<IActionResult> Index()
        {
            var danhSachSanPham = await _context.SanPhams
                .Include(sp => sp.LoaiSanPham)
                .ToListAsync();

            return View(danhSachSanPham);
        }


        public async Task<IActionResult> PhuPhamTho()
        {
            var danhSachSanPham = await _context.SanPhams
                .Include(sp => sp.LoaiSanPham)
                .ToListAsync();

            return View(danhSachSanPham);
        }

        public async Task<IActionResult> DaQuaXuLy()
        {
            var danhSachSanPham = await _context.SanPhams
                .Include(sp => sp.LoaiSanPham)
                .ToListAsync();

            return View(danhSachSanPham);
        }

        public async Task<IActionResult> ThucAnChanNuoi()
        {
            var danhSachSanPham = await _context.SanPhams
                .Include(sp => sp.LoaiSanPham)
                .ToListAsync();

            return View(danhSachSanPham);
        }

        public async Task<IActionResult> PhanBon()
        {
            var danhSachSanPham = await _context.SanPhams
                .Include(sp => sp.LoaiSanPham)
                .ToListAsync();

            return View(danhSachSanPham);
        }

        public async Task<IActionResult> NangLuongSinhKhoi()
        {
            var danhSachSanPham = await _context.SanPhams
                .Include(sp => sp.LoaiSanPham)
                .ToListAsync();

            return View(danhSachSanPham);
        }
        // =========================================
        // 🧡 CHI TIẾT SẢN PHẨM + YÊU THÍCH
        // =========================================
        public async Task<IActionResult> CT_SP(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Mã sản phẩm không hợp lệ.");
            }

            var product = await _context.SanPhams
                .Include(sp => sp.LoaiSanPham)
                .Include(sp => sp.DonViTinh)
                .Include(sp => sp.ChiTietDanhGias)
                    .ThenInclude(ctdg => ctdg.KhachHang)
                .FirstOrDefaultAsync(sp => sp.M_SanPham == id);

            if (product == null)
                return NotFound("Không tìm thấy sản phẩm.");

            var tonKho = await _context.LoTonKhos
                .Where(tk => tk.M_SanPham == id)
                .Select(tk => tk.KhoiLuongConLai)
                .FirstOrDefaultAsync();

            ViewData["SoLuongTonKho"] = tonKho;
            ViewData["Title"] = $"Chi tiết: {product.TenSanPham}";

            var relatedProducts = await _context.SanPhams
                .Where(sp => sp.M_LoaiSP == product.M_LoaiSP && sp.M_SanPham != id)
                .Include(sp => sp.LoaiSanPham)
                .OrderByDescending(sp => sp.NgayTao)
                .Take(4)
                .ToListAsync();

            ViewData["RelatedProducts"] = relatedProducts;
            ViewData["DefaultQuantity"] = 1;

            // Xác định sản phẩm này có trong danh sách yêu thích của user không
            bool isFavorite = false;
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                isFavorite = await _context.SanPhamYeuThichs
                    .AnyAsync(y => y.UserId == userId && y.M_SanPham == id);
            }
            ViewData["IsFavorite"] = isFavorite;

            return View(product);
        }

        // =========================================
        // 🩷 BẬT / TẮT YÊU THÍCH
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken] // <-- THAY ĐỔI DUY NHẤT TRONG FILE NÀY
        public async Task<IActionResult> ToggleWishlist(string m_SanPham)
        {
            if (!User.Identity.IsAuthenticated)
                return Json(new { success = false, message = "Bạn cần đăng nhập để sử dụng tính năng này!" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existing = await _context.SanPhamYeuThichs
                .FirstOrDefaultAsync(x => x.UserId == userId && x.M_SanPham == m_SanPham);

            bool isFavorite = false;

            if (existing != null)
            {
                // Đã có => XÓA
                _context.SanPhamYeuThichs.Remove(existing);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Chưa có => THÊM
                var newFav = new SanPhamYeuThich
                {
                    M_YeuThich = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                    UserId = userId,
                    M_SanPham = m_SanPham,
                    NgayThem = DateTime.UtcNow
                };
                _context.SanPhamYeuThichs.Add(newFav);
                await _context.SaveChangesAsync();
                isFavorite = true;
            }

            int total = await _context.SanPhamYeuThichs.CountAsync(x => x.UserId == userId);

            return Json(new
            {
                success = true,
                isFavorite,
                total,
                message = isFavorite ? "Đã thêm vào yêu thích ❤️" : "Đã xóa khỏi yêu thích 💔"
            });
        }

        // ✅ Lấy danh sách mini yêu thích (hiện trên header)
        [HttpGet]
        public async Task<IActionResult> GetWishlistMini()
        {
            if (!User.Identity.IsAuthenticated)
                return Json(new { items = new List<object>(), total = 0 });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var items = await _context.SanPhamYeuThichs
                .Include(w => w.SanPham)
                .Where(w => w.UserId == userId)
                .Select(w => new
                {
                    w.M_SanPham,
                    TenSanPham = w.SanPham.TenSanPham,
                    HinhAnh = w.SanPham.AnhSanPham,
                    Gia = w.SanPham.Gia
                })
                .ToListAsync();

            return Json(new { items, total = items.Count });
        }

        // =========================================
        // 📝 GỬI ĐÁNH GIÁ (Giữ nguyên code của bạn)
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(ChiTietDanhGia reviewInput)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["ReviewMessage"] = "Lỗi: Bạn cần đăng nhập để gửi đánh giá.";
                return RedirectToAction("CT_SP", new { id = reviewInput.M_SanPham });
            }

            if (!int.TryParse(reviewInput.MucDoHaiLong, out int rating) || rating < 1 || rating > 5)
            {
                ModelState.AddModelError("MucDoHaiLong", "Mức độ hài lòng không hợp lệ.");
            }

            if (ModelState.IsValid)
            {
                var productExists = await _context.SanPhams.AnyAsync(p => p.M_SanPham == reviewInput.M_SanPham);
                if (!productExists)
                {
                    TempData["ReviewMessage"] = "Lỗi: Sản phẩm không tồn tại.";
                    return RedirectToAction("Index", "Home");
                }

                var customerExists = await _context.KhachHangs.AnyAsync(k => k.M_KhachHang == reviewInput.M_KhachHang);
                if (!customerExists)
                {
                    TempData["ReviewMessage"] = "Lỗi: Khách hàng không tồn tại.";
                    return RedirectToAction("CT_SP", new { id = reviewInput.M_SanPham });
                }

                bool alreadyReviewed = await _context.ChiTietDanhGias
                    .AnyAsync(dg => dg.M_SanPham == reviewInput.M_SanPham &&
                                    dg.M_KhachHang == reviewInput.M_KhachHang);

                if (alreadyReviewed)
                {
                    TempData["ReviewMessage"] = "Thông báo: Bạn đã đánh giá sản phẩm này rồi.";
                    return RedirectToAction("CT_SP", new { id = reviewInput.M_SanPham });
                }

                reviewInput.NgayDanhGia = DateTime.UtcNow;
                _context.ChiTietDanhGias.Add(reviewInput);
                await _context.SaveChangesAsync();

                TempData["ReviewMessage"] = "Cảm ơn bạn đã gửi đánh giá!";
                return RedirectToAction("CT_SP", new { id = reviewInput.M_SanPham });
            }

            TempData["ReviewMessage"] = "Đánh giá không hợp lệ. Vui lòng kiểm tra lại thông tin.";
            return RedirectToAction("CT_SP", new { id = reviewInput.M_SanPham });
        }
    }
}