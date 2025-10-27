using DACS.Models; // <= THAY THẾ bằng namespace chứa Models của bạn
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Giả sử bạn dùng Identity và tên User model là ApplicationUser

namespace DACS.Controllers
{
    [Authorize] // Bắt buộc người dùng phải đăng nhập
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context; // <= THAY TÊN DbContext nếu khác
        private readonly UserManager<ApplicationUser> _userManager; // <= THAY TÊN ApplicationUser nếu khác

        public WishlistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Wishlist hoặc /Wishlist/Index
        public async Task<IActionResult> Index()
        {
            // 1. Lấy thông tin người dùng Identity hiện tại
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // Chuyển hướng đến trang đăng nhập
            }

            // 2. Lấy UserId (khóa chính của Identity) từ model của bạn
            var currentUserId = user.Id; // <-- THAY ĐỔI QUAN TRỌNG

            // 3. Truy vấn các sản phẩm yêu thích bằng UserId
            var wishlistProducts = await _context.SanPhamYeuThichs
                .Where(w => w.UserId == currentUserId) // <-- SỬ DỤNG UserId
                .Include(w => w.SanPham) // Tải thông tin SanPham
                    .ThenInclude(sp => sp.DonViTinh) // Tải luôn thông tin ĐVT
                .Select(w => w.SanPham) // Chỉ chọn đối tượng SanPham
                .ToListAsync();

            return View(wishlistProducts);
        }
    }
}