using DACS.Extention; // Đổi YourNameSpace.Extensions thành DACS.Extention
using DACS.Models;
using DACS.Repositories; // Đổi YourNameSpace thành DACS
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System; // Thêm using
using System.Collections.Generic; // Thêm using
using System.Linq; // Thêm using
using System.Threading.Tasks;
using YourNameSpace.Extensions; // Thêm using
using DACS.Services;

namespace DACS.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly ISanPhamRepository _productRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ShoppingCartController> _logger;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;

        public ShoppingCartController(ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            ISanPhamRepository productRepository, 
            ILogger<ShoppingCartController> logger,
            IEmailService emailService,
            ISmsService smsService)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _emailService = emailService;
            _smsService = smsService;
        }

        public IActionResult Checkout()
        {
            return View(new DonHang());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(DonHang order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            var nguoiMuaProfile = await _context.KhachHangs.FirstOrDefaultAsync(kh => kh.UserId == user.Id);
            if (nguoiMuaProfile == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hồ sơ khách hàng của bạn.";
                return RedirectToAction("Index");
            }

            var errorMessages = new List<string>();

            // <<< ================= SỬA LỖI LOGIC TỒN KHO ================= >>>
            foreach (var item in cart.Items)
            {
                // Lấy TỔNG tồn kho của sản phẩm này (từ TẤT CẢ các lô)
                var tongTonKho = await _context.LoTonKhos
                    .Where(t => t.M_SanPham == item.ProductId)
                    .SumAsync(t => t.KhoiLuongConLai); // Tính TỔNG khối lượng còn lại

                // Sửa "item.KhoiluongConLai" thành "item.Khoiluong"
                if ((float)item.Khoiluong > (float)tongTonKho)
                {
                    // Sửa "item.KhoiluongConLai" thành "item.Khoiluong" trong thông báo lỗi
                    errorMessages.Add($"Sản phẩm '{item.Name}' chỉ còn {tongTonKho:N0}kg, bạn đặt {item.Khoiluong:N0}kg.");
                }
            }
            // <<< ================= KẾT THÚC SỬA LỖI ================= >>>

            if (errorMessages.Any())
            {
                ViewBag.CartErrors = errorMessages;
                return View("Index", cart); // Trả về trang giỏ hàng kèm lỗi
            }

            // (Phần logic tạo Đơn Hàng, Vận Đơn, Chi Tiết của bạn đã ổn, giữ nguyên)
            // (Lưu ý: Bạn nên bọc toàn bộ phần này trong một Transaction)

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ======= TẠO MÃ VẬN ĐƠN ========
                string vanDonId = "VD" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                var vanChuyenExist = await _context.VanChuyens.FirstOrDefaultAsync(vc => vc.M_VanDon == vanDonId);
                if (vanChuyenExist == null)
                {
                    vanChuyenExist = new VanChuyen
                    {
                        M_VanDon = vanDonId,
                        DonViVanChuyen = "DHL" // Nên lấy từ form
                    };
                    _context.VanChuyens.Add(vanChuyenExist);
                    await _context.SaveChangesAsync();
                }

                // ======= TẠO MÃ ĐƠN HÀNG ========
                var lastOrder = await _context.DonHangs.OrderByDescending(o => o.M_DonHang).FirstOrDefaultAsync();
                int nextNumber = 1;
                if (lastOrder != null && lastOrder.M_DonHang.StartsWith("DH"))
                {
                    var numberPart = lastOrder.M_DonHang.Substring(2);
                    if (int.TryParse(numberPart, out int parsedNumber))
                    {
                        nextNumber = parsedNumber + 1;
                    }
                }

                order.M_DonHang = "DH" + nextNumber.ToString("D6");
                order.M_VanDon = vanChuyenExist.M_VanDon;
                order.TrangThai = order.TrangThai ?? "Chờ xác nhận"; // <<< SỬA: Dùng "Chờ xác nhận"
                order.M_KhachHang = nguoiMuaProfile.M_KhachHang;
                order.NgayDat = DateTime.UtcNow;
                order.TotalPrice = cart.Items.Sum(i => i.Price * i.Khoiluong);
                order.TrangThaiThanhToan = "Chưa thanh toán";

                _context.DonHangs.Add(order);
                await _context.SaveChangesAsync();

                // ======= CHI TIẾT ĐƠN HÀNG ========
                order.ChiTietDatHangs = cart.Items.Select(i => new ChiTietDatHang
                {
                    M_KhachHang = nguoiMuaProfile.M_KhachHang,
                    M_DonHang = order.M_DonHang,
                    M_SanPham = i.ProductId,
                    ProductId = i.ProductId, // Giả định bạn có 2 cột giống nhau?
                    Khoiluong = i.Khoiluong,
                    GiaDatHang = i.Price,
                    M_CTDatHang = "CT" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                    TongTien = (long)(i.Price * i.Khoiluong),
                    NgayTao = DateTime.UtcNow,
                    Quantity = i.Quantity, // Quantity (số lượng) này dùng để làm gì?
                    TrangThaiDonHang = "Chờ xác nhận" // <<< SỬA
                }).ToList();

                _context.ChiTietDatHangs.AddRange(order.ChiTietDatHangs);
                await _context.SaveChangesAsync();

                // (Lưu ý: Logic trừ kho FIFO nên được gọi ở đây, nhưng bạn đang gọi nó ở Controller khác)

                await transaction.CommitAsync(); // Hoàn thành
                // <<< ============ GỬI EMAIL SAU KHI MUA HÀNG THÀNH CÔNG ============ >>>
                try
                {
                    var subject = $"Xác nhận đơn hàng #{order.M_DonHang}";
                    var body = $@"
                        <h1>Cảm ơn bạn đã mua hàng!</h1>
                        <p>Chào {user.FullName ?? user.UserName},</p>
                        <p>Đơn hàng <strong>#{order.M_DonHang}</strong> của bạn đã được tiếp nhận và đang chờ xử lý.</p>
                        <p><strong>Tổng giá trị đơn hàng:</strong> {order.TotalPrice:N0} VNĐ</p>
                        <p><strong>Địa chỉ giao hàng:</strong> {order.ShippingAddress},{order.SoDienThoaidathang}</p>
                        <p>Chúng tôi sẽ liên hệ với bạn sớm nhất.</p>
                        <p>Trân trọng,</p>
                        <p>Đội ngũ [Tên Cửa Hàng]</p>";

                    await _emailService.SendEmailAsync(user.Email, subject, body);
                }
                catch (Exception emailEx)
                {
                    // Ghi log lỗi gửi mail nhưng không làm ảnh hưởng đến kết quả trả về cho khách
                    _logger.LogError(emailEx, "Lỗi khi gửi email xác nhận đơn hàng {DonHangId}", order.M_DonHang);
                }
                // <<< ================== KẾT THÚC GỬI EMAIL ================== >>>

                // <<< ============ GỬI SMS SAU KHI MUA HÀNG ============ >>>
                try
                {
                    // Dùng SĐT từ Đơn Hàng (order) thay vì từ Tài Khoản (user)
                    if (!string.IsNullOrEmpty(order.SoDienThoaidathang))
                    {
                        var smsMessage = $"Cam on ban da mua hang. Don hang #{order.M_DonHang} da duoc tiep nhan.";

                        // Dùng order.PhoneNumber
                        await _smsService.SendSmsAsync(order.SoDienThoaidathang, smsMessage);
                    }
                }
                catch (Exception smsEx)
                {
                    _logger.LogError(smsEx, "Lỗi khi gửi SMS cho đơn hàng {DonHangId}", order.M_DonHang);
                }
                // <<< ================== KẾT THÚC GỬI SMS ================== >>>
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Hoàn tác nếu lỗi
                _logger.LogError(ex, "Lỗi nghiêm trọng khi Checkout ĐH {DonHangId}", order.M_DonHang);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tạo đơn hàng. Vui lòng thử lại.";
                return View("Index", cart);
            }

            HttpContext.Session.Remove("Cart");
            return View("OrderCompleted", order.M_DonHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCartItem([FromBody] CartUpdateModel model)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null) return BadRequest();

            var item = cart.Items.FirstOrDefault(i => i.ProductId == model.ProductId);
            if (item != null)
            {
                item.Khoiluong = model.Khoiluong;
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return Ok();
        }

        public class CartUpdateModel
        {
            public string ProductId { get; set; }
            public float Khoiluong { get; set; }
        }

        // <<< SỬA: Đã sửa lại tham số (bỏ quantity)
        public async Task<IActionResult> AddToCart(string productId, float khoiluong)
        {
            var product = await GetProductFromDatabase(productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("Index", "Home"); // Hoặc trang sản phẩm
            }

            var cartItem = new CartItem
            {
                ProductId = product.M_SanPham,
                Name = product.TenSanPham,
                Price = product.Gia,
                Quantity = 1, // Tạm gán là 1 (vì đang bán theo khối lượng)
                Khoiluong = khoiluong // <<< SỬA: Lấy từ tham số
            };
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart);
        }

        private async Task<SanPham> GetProductFromDatabase(string productId)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            return product;
        }

        public IActionResult RemoveFromCart(string productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart is not null)
            {
                cart.RemoveItem(productId);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }
    }
}