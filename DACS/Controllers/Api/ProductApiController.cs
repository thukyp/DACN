using DACS.Models;
using DACS.Repositories;
using DACS.Repository; // Sử dụng Namespace Repository của bạn
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DACS.Controllers.Api
{
    [Route("api/[controller]")] // Đường dẫn sẽ là: domain/api/productapi
    [ApiController]
    public class ProductApiController : ControllerBase
    {
        private readonly ISanPhamRepository _sanPhamRepo;
        // Hoặc dùng ApplicationDbContext nếu Repository chưa đủ method
        private readonly ApplicationDbContext _context;

        public ProductApiController(ISanPhamRepository sanPhamRepo, ApplicationDbContext context)
        {
            _sanPhamRepo = sanPhamRepo;
            _context = context;
        }

        // 1. API Lấy tất cả sản phẩm
        // GET: api/productapi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = _context.SanPhams.ToList();

            // Xử lý ảnh: Mobile cần đường dẫn tuyệt đối (VD: http://192.168.1.10:5000/images/sp1.jpg)
            // Bạn cần nối chuỗi domain vào đường dẫn ảnh ở đây nếu lưu đường dẫn tương đối.

            return Ok(products); // Trả về JSON
        }

        // 2. API Tìm kiếm (Dựa trên logic TimKiem cũ của bạn)
        // GET: api/productapi/search?keyword=abc
        [HttpGet("search")]
        public IActionResult Search(string keyword)
        {
            var query = _context.SanPhams.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(sp => sp.TenSanPham.Contains(keyword));
            }

            var result = query.ToList();
            return Ok(result);
        }

        // 3. API Chi tiết sản phẩm
        // GET: api/productapi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var product = await _context.SanPhams.FindAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }
        [HttpGet("products/{id}/batches")]
        public async Task<IActionResult> GetProductBatches(string id)
        {
            var batches = await _context.LoTonKhos
                .Where(l => l.M_SanPham == id && l.KhoiLuongConLai > 0)
                .Select(l => new {
                    maLo = l.MaLoTonKho,
                    ngayNhap = l.NgayNhapKho,
                    hanSuDung = l.HanSuDung,
                    tonKho = l.KhoiLuongConLai
                })
                .ToListAsync();

            return Ok(batches);
        }
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

        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetProductDetail(string id)
        {
            // 1. Tìm sản phẩm (Trim ID để tránh lỗi khoảng trắng từ Client)
            var productId = id.Trim();
            var sp = await _context.SanPhams
                .Include(s => s.LoaiSanPham)
                .Include(s => s.DonViTinh)
                .FirstOrDefaultAsync(s => s.M_SanPham == productId);

            if (sp == null) return NotFound(new { message = "Không tìm thấy sản phẩm" });

            // 2. Tính tổng tồn kho từ bảng LoTonKhos
            // Chỉ tính những lô có số lượng lớn hơn 0
            var totalStock = await _context.LoTonKhos
                .Where(l => l.M_SanPham == productId && l.KhoiLuongConLai > 0)
                .SumAsync(l => (double?)l.KhoiLuongConLai) ?? 0; // Sử dụng cast nullable để tránh lỗi nếu ko có dòng nào

            return Ok(new
            {
                m_SanPham = sp.M_SanPham,
                tenSanPham = sp.TenSanPham,
                gia = sp.Gia,
                anhSanPham = sp.AnhSanPham,
                tenLoai = sp.LoaiSanPham?.TenLoai,
                tenDVT = sp.DonViTinh?.TenLoaiTinh,
                moTa = sp.MoTa,
                totalStock = totalStock // Trả về số lượng thực tế trong kho
            });
        }
    }
}