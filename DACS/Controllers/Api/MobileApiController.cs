using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DACS.Models; // Hãy đảm bảo namespace này đúng với project của bạn
using System.Security.Claims;

namespace DACS.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class MobileApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MobileApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 1. DANH SÁCH SẢN PHẨM ---
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.SanPhams
                .Include(sp => sp.LoaiSanPham)
                .Include(sp => sp.DonViTinh)
                .Select(sp => new {
                    m_SanPham = sp.M_SanPham,
                    tenSanPham = sp.TenSanPham,
                    gia = sp.Gia,
                    // Đảm bảo đường dẫn ảnh chuẩn để Flutter không bị lỗi 404
                    anhSanPham = sp.AnhSanPham,
                    tenLoai = sp.LoaiSanPham != null ? sp.LoaiSanPham.TenLoai : "",
                    tenDVT = sp.DonViTinh != null ? sp.DonViTinh.TenLoaiTinh : "kg",
                    moTa = sp.MoTa
                }).ToListAsync();
            return Ok(products);
        }

        // --- 2. CHI TIẾT SẢN PHẨM & TỔNG TỒN KHO ---
        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetProductDetail(string id)
        {
            var sp = await _context.SanPhams
                .Include(s => s.LoaiSanPham)
                .Include(s => s.DonViTinh)
                .FirstOrDefaultAsync(s => s.M_SanPham == id);

            if (sp == null) return NotFound();

            // Tính tổng tồn kho từ tất cả các lô hàng (FIFO)
            var totalStock = await _context.LoTonKhos
                .Where(l => l.M_SanPham == id)
                .SumAsync(l => l.KhoiLuongConLai);

            return Ok(new
            {
                m_SanPham = sp.M_SanPham,
                tenSanPham = sp.TenSanPham,
                gia = sp.Gia,
                anhSanPham = sp.AnhSanPham,
                tenLoai = sp.LoaiSanPham?.TenLoai,
                tenDVT = sp.DonViTinh?.TenLoaiTinh,
                moTa = sp.MoTa,
                totalStock
            });
        }

        // --- 3. GIỎ HÀNG (Lấy theo UserId) ---
        //[HttpGet("cart/{userId}")]
        //public async Task<IActionResult> GetCart(string userId)
        //{
        //    var items = await _context.CartItems
        //        .Where(c => c.UserId == userId)
        //        .Select(c => new {
        //            c.ProductId,
        //            c.Name,
        //            c.Price,
        //            c.Khoiluong,
        //            c.Quantity
        //        })
        //        .ToListAsync();
        //    return Ok(items);
        //}

        // --- 4. YÊU THÍCH (Wishlist) ---
        [HttpGet("wishlist/{userId}")]
        public async Task<IActionResult> GetWishlist(string userId)
        {
            var wishlist = await _context.SanPhamYeuThichs
                .Include(y => y.SanPham)
                .Where(y => y.UserId == userId)
                .Select(y => new {
                    y.M_SanPham,
                    tenSanPham = y.SanPham.TenSanPham,
                    gia = y.SanPham.Gia,
                    anhSanPham = y.SanPham.AnhSanPham
                })
                .ToListAsync();
            return Ok(wishlist);
        }

        // --- 5. ĐĂNG KÝ THU GOM ---
        [HttpPost("thugom/request")]
        public async Task<IActionResult> CreateThuGom([FromBody] ChiTietThuGom request)
        {
            if (request == null) return BadRequest();

            // Gán mã thu gom tự động nếu cần
            request.M_YeuCau = "TG" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            _context.ChiTietThuGoms.Add(request);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Yêu cầu thu gom đã được gửi" });
        }
    }
}