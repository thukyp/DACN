using DACS.Models;
using DACS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DACS.Controllers
{
    public class DS_SPController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISanPhamRepository _sanphamRepository;

        public DS_SPController(ApplicationDbContext context, ISanPhamRepository sanphamRepository)
        {
            _context = context;
            _sanphamRepository = sanphamRepository;
        }
        public IActionResult TimKiem(string keyword)
        {
            var query = _context.SanPhams
                .Include(sp => sp.LoaiSanPham) // Giải quyết lỗi null
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
                .Select(sp => new {
                    m_SanPham = sp.M_SanPham,
                    tenSanPham = sp.TenSanPham
                })
                .Take(5) // giới hạn số lượng gợi ý
                .ToList();

            return Json(result);
        }

        public async Task<IActionResult> Index()
        {
            var danhSachSanPham = await _context.SanPhams
                .Include(sp => sp.LoaiSanPham) // Include để LoaiSanPham không bị null
                .ToListAsync();

            return View(danhSachSanPham);
        }

        public async Task<IActionResult> PhuPhamTho()
        {
            var danhSachSanPham = await _context.SanPhams
                .Include(sp => sp.LoaiSanPham) // Include để LoaiSanPham không bị null
                .ToListAsync();

            return View(danhSachSanPham);
        }
        public async Task<IActionResult> DaQuaXuLy()
        {
            var danhSachSanPham = await _context.SanPhams
                .Include(sp => sp.LoaiSanPham) // Include để LoaiSanPham không bị null
                .ToListAsync();

            return View(danhSachSanPham);
        }
        public async Task<IActionResult> ThucAnChanNuoi()
        {
            var danhSachSanPham = await _context.SanPhams
                .Include(sp => sp.LoaiSanPham) // Include để LoaiSanPham không bị null
                .ToListAsync();

            return View(danhSachSanPham);
        }
        public async Task<IActionResult> PhanBon()
        {
            var danhSachSanPham = await _context.SanPhams
                .Include(sp => sp.LoaiSanPham) // Include để LoaiSanPham không bị null
                .ToListAsync();

            return View(danhSachSanPham);
        }

        public async Task<IActionResult> NangLuongSinhKhoi()
        {
            var danhSachSanPham = await _context.SanPhams
                .Include(sp => sp.LoaiSanPham) // Include để LoaiSanPham không bị null
                .ToListAsync();

            return View(danhSachSanPham);
        }

        public async Task<IActionResult> CT_SP(string id) // id is M_SanPham
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Mã sản phẩm không hợp lệ.");
            }

            var product = await _context.SanPhams
                                        .Include(sp => sp.LoaiSanPham) // For category name
                                        .Include(sp => sp.DonViTinh)   // For unit name
                                        .Include(sp => sp.ChiTietDanhGias) // Include reviews
                                            .ThenInclude(ctdg => ctdg.KhachHang) // Include customer details for reviews
                                        .FirstOrDefaultAsync(sp => sp.M_SanPham == id);
            var tonKho = await _context.TonKhos
                           .Where(tk => tk.M_SanPham == id)
                           .Select(tk => tk.KhoiLuong)
                           .FirstOrDefaultAsync();
            ViewData["SoLuongTonKho"] = tonKho;
            
            if (product == null)
            {
                return NotFound("Không tìm thấy sản phẩm.");
            }

            ViewData["Title"] = $"Chi tiết: {product.TenSanPham}";

            // Related products (example: 4 from the same category, excluding current)
            var relatedProducts = await _context.SanPhams
                                            .Where(sp => sp.M_LoaiSP == product.M_LoaiSP && sp.M_SanPham != id)
                                            .Include(sp => sp.LoaiSanPham) // Include LoaiSanPham for related products display
                                            .OrderByDescending(sp => sp.NgayTao) // Example: newest first
                                            .Take(4)
                                            .ToListAsync();
            ViewData["RelatedProducts"] = relatedProducts;
            ViewData["DefaultQuantity"] = 1;

            // For the review form, pre-fill M_KhachHang if user is logged in
            // This is an EXAMPLE. You need to implement your actual customer ID retrieval.
            if (User.Identity.IsAuthenticated)
            {
                // If using ASP.NET Core Identity, User.FindFirstValue(ClaimTypes.NameIdentifier) gets the user ID.
                // You might need a mapping from this User ID to your KhachHang.M_KhachHang.
                // For demonstration, let's assume you have a way to get M_KhachHang.
                // string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                // KhachHang currentUserKhachHang = await _context.KhachHangs.FirstOrDefaultAsync(kh => kh.IdentityUserId == currentUserId);
                // if (currentUserKhachHang != null) {
                //     ViewData["CurrentUser_M_KhachHang"] = currentUserKhachHang.M_KhachHang;
                // }
                // For now, as a placeholder if you don't have Identity setup for M_KhachHang directly:
                ViewData["CurrentUser_M_KhachHang"] = "KH001"; // <<<<----- IMPORTANT: REPLACE THIS WITH ACTUAL LOGIC
            }


            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(ChiTietDanhGia reviewInput)
        {


            if (User.Identity.IsAuthenticated)
            {

            }
            else
            {
                TempData["ReviewMessage"] = "Lỗi: Bạn cần đăng nhập để gửi đánh giá.";
                return RedirectToAction("CT_SP", new { id = reviewInput.M_SanPham });
            }


            // Basic validation for MucDoHaiLong (string to int conversion for logic)
            if (!int.TryParse(reviewInput.MucDoHaiLong, out int rating) || rating < 1 || rating > 5)
            {
                ModelState.AddModelError("MucDoHaiLong", "Mức độ hài lòng không hợp lệ.");
            }

            if (ModelState.IsValid)
            {
                // Check if product exists
                var productExists = await _context.SanPhams.AnyAsync(p => p.M_SanPham == reviewInput.M_SanPham);
                if (!productExists)
                {
                    TempData["ReviewMessage"] = "Lỗi: Sản phẩm không tồn tại.";
                    return RedirectToAction("Index", "Home"); // Or a general error page
                }

                // Check if customer exists (if M_KhachHang is not from a trusted source like claims)
                var customerExists = await _context.KhachHangs.AnyAsync(k => k.M_KhachHang == reviewInput.M_KhachHang);
                if (!customerExists)
                {
                    TempData["ReviewMessage"] = "Lỗi: Khách hàng không tồn tại.";
                    return RedirectToAction("CT_SP", new { id = reviewInput.M_SanPham });
                }


                // Check if user has already reviewed this product (optional, depends on your business logic)
                bool alreadyReviewed = await _context.ChiTietDanhGias
                                               .AnyAsync(dg => dg.M_SanPham == reviewInput.M_SanPham &&
                                                               dg.M_KhachHang == reviewInput.M_KhachHang);
                if (alreadyReviewed)
                {
                    TempData["ReviewMessage"] = "Thông báo: Bạn đã đánh giá sản phẩm này rồi.";
                    return RedirectToAction("CT_SP", new { id = reviewInput.M_SanPham });
                }

                reviewInput.NgayDanhGia = DateTime.UtcNow; // Set the review date

                _context.ChiTietDanhGias.Add(reviewInput);
                await _context.SaveChangesAsync();

                TempData["ReviewMessage"] = "Cảm ơn bạn đã gửi đánh giá!";
                return RedirectToAction("CT_SP", new { id = reviewInput.M_SanPham });
            }

            // If ModelState is invalid, prepare data to re-render the CT_SP view with errors
            TempData["ReviewMessage"] = "Đánh giá không hợp lệ. Vui lòng kiểm tra lại thông tin.";
            // We need to reload the product for the view if we were to return View("CT_SP", product)
            // But redirecting is simpler here. The CT_SP action will handle reloading.
            return RedirectToAction("CT_SP", new { id = reviewInput.M_SanPham });
        }
    }
}

