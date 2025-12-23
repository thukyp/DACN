using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DACS.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using DACS.Services;
using DACS.Models.ViewModels;

namespace DACS.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu đăng nhập để gửi yêu cầu thu gom
    public class ThuGomApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<ThuGomApiController> _logger;

        public ThuGomApiController(ApplicationDbContext context, IEmailService emailService, ILogger<ThuGomApiController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // 1. Lấy danh sách yêu cầu của tôi
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(kh => kh.UserId == userId);
            if (khachHang == null) return NotFound("Khách hàng không tồn tại.");

            var requests = await _context.YeuCauThuGoms
                .Where(y => y.M_KhachHang == khachHang.M_KhachHang)
                .Include(y => y.ChiTietThuGoms)
                .OrderByDescending(y => y.NgayYeuCau)
                .ToListAsync();

            return Ok(requests);
        }

        // 2. Gửi yêu cầu thu gom mới (không kèm ảnh trong JSON - dùng Multipart nếu muốn kèm ảnh)
        [HttpPost("create")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateThuGomDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(kh => kh.UserId == userId);
            if (khachHang == null) return Unauthorized("Không tìm thấy thông tin khách hàng.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tạo mã yêu cầu (Logic lấy từ ThuGomController)
                string requestCode = GenerateRequestCode();

                var yeuCau = new YeuCauThuGom
                {
                    M_YeuCau = requestCode,
                    M_KhachHang = khachHang.M_KhachHang,
                    NgayYeuCau = DateTime.UtcNow,
                    TrangThai = "Chờ xử lý",
                    MaTinh = dto.SupplierProvince,
                    MaQuan = dto.SupplierDistrict,
                    MaXa = dto.SupplierWard,
                    DiaChi_DuongApThon = dto.SupplierStreet,
                    ThoiGianSanSang = dto.PickupReadyTime,
                    GhiChu = dto.SupplierNotes
                };

                var chiTiet = new ChiTietThuGom
                {
                    M_ChiTiet = Guid.NewGuid().ToString("N").Substring(0, 10),
                    M_YeuCau = requestCode,
                    M_LoaiSP = dto.M_LoaiSP,
                    M_SanPham = dto.M_SanPham,
                    M_DonViTinh = dto.ByproductUnit,
                    SoLuong = (int)dto.ByproductQuantity,
                    MoTa = dto.ByproductDescription,
                    GiaTriMongMuon = dto.ByproductValue,
                    DacTinh_CongKenh = dto.CharBulky,
                    DacTinh_AmUot = dto.CharAmUot,
                    // ... ánh xạ các đặc tính khác
                    TrangThaiXuLy = "MoiYeuCau"
                };

                _context.YeuCauThuGoms.Add(yeuCau);
                _context.ChiTietThuGoms.Add(chiTiet);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Gửi email thông báo (tùy chọn)
                _ = _emailService.SendEmailAsync(khachHang.Email_KhachHang, "Xác nhận thu gom", $"Mã yêu cầu: {requestCode}");

                return Ok(new { message = "Gửi yêu cầu thành công", requestCode });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi hệ thống: " + ex.Message);
            }
        }

        private string GenerateRequestCode()
        {
            string prefix = "YC" + DateTime.Now.ToString("yyMMdd");
            var lastCode = _context.YeuCauThuGoms
                .Where(y => y.M_YeuCau.StartsWith(prefix))
                .OrderByDescending(y => y.M_YeuCau)
                .Select(y => y.M_YeuCau)
                .FirstOrDefault();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCode))
            {
                int.TryParse(lastCode.Substring(prefix.Length), out nextNumber);
                nextNumber++;
            }
            return prefix + nextNumber.ToString("D2");
        }
    }
}