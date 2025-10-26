using DACS.Areas.Owner.Models;
using DACS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace DACS.Areas.QuanLyDH.Controllers
{
    [Area("QuanLyDH")]
    [Authorize(Roles = SD.Role_Owner + "," + SD.Role_QuanLyDH)] // Đảm bảo phân quyền phù hợp
    public class QuanLyThanhToanController : Controller
    {
        private readonly ApplicationDbContext _context;

        private const string StatusPendingDbValue = "Chờ xác nhận"; // Hoặc "Chưa xử lý" nếu bạn dùng nó làm giá trị DB ban đầu
        private const string StatusConfirmedDbValue = "Đã xác nhận";
        private const string StatusProcessingDbValue = "Đang xử lý"; // Đã có
        private const string StatusShippingDbValue = "Đang giao hàng";
        private const string StatusCompletedDbValue = "Hoàn thành";
        private const string StatusCancelledDbValue = "Đã hủy";


        public QuanLyThanhToanController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search,
            DateTime? startDate,
            DateTime? endDate,
            string orderStatus, // Filter theo trạng thái đơn hàng
            string paymentStatus, // Filter theo trạng thái thanh toán
            int page = 1)
        {
            ViewData["Title"] = "Quản lý thanh toán";
            int pageSize = 10;

            var viewModel = new OrderManagementViewModel // Đảm bảo OrderManagementViewModel được định nghĩa đúng
            {
                SearchInput = search,
                DateFromFilter = startDate,
                DateToFilter = endDate,
                SelectedOrderStatus = orderStatus,
                SelectedPaymentStatus = paymentStatus,
                OrderStatusOptions = GetOrderStatusOptions(orderStatus),
                PaymentStatusOptions = GetPaymentStatusOptions(paymentStatus), // Sử dụng hàm GetPaymentStatusOptions đã có
                CurrentPage = page
            };

            IQueryable<DonHang> donHangQuery = _context.DonHangs
                .Include(dh => dh.KhachHang)
                .Include(dh => dh.PhuongThucThanhToan) // Đảm bảo include để lấy tên phương thức TT
                .OrderByDescending(dh => dh.NgayDat);

            if (!string.IsNullOrEmpty(search))
            {
                string searchLower = search.ToLower();
                donHangQuery = donHangQuery.Where(dh =>
                    dh.M_DonHang.ToLower().Contains(searchLower) ||
                    (dh.KhachHang != null && dh.KhachHang.Ten_KhachHang.ToLower().Contains(searchLower)) ||
                    (dh.KhachHang != null && dh.KhachHang.SDT_KhachHang.Contains(search))
                );
            }
            if (startDate.HasValue)
            {
                donHangQuery = donHangQuery.Where(dh => dh.NgayDat.Date >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                donHangQuery = donHangQuery.Where(dh => dh.NgayDat.Date <= endDate.Value.Date);
            }
            if (!string.IsNullOrEmpty(orderStatus)) // Lọc theo trạng thái đơn hàng
            {
                string dbOrderStatus = MapFilterToDbOrderStatus(orderStatus); // Hàm này đã có
                if (!string.IsNullOrEmpty(dbOrderStatus))
                {
                    donHangQuery = donHangQuery.Where(dh => dh.TrangThai == dbOrderStatus);
                }
            }
            // Lọc theo trạng thái thanh toán (dựa trên TrangThaiThanhToan)
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                // Giả sử paymentStatus là giá trị text như "Chưa thanh toán", "Đã thanh toán"
                // Nếu bạn có hàm MapFilterToDbPaymentStatus thì dùng, nếu không thì so sánh trực tiếp
                donHangQuery = donHangQuery.Where(dh => dh.TrangThaiThanhToan == paymentStatus);
            }

            // Phần thống kê có thể không cần thiết trên trang quản lý thanh toán, hoặc bạn có thể tùy chỉnh lại
            // viewModel.PendingConfirmationOrders = ... 
            // viewModel.ProcessingOrShippingOrders = ...
            // viewModel.CompletedOrdersThisMonth = ...
            // viewModel.CancelledOrdersThisMonth = ...

            int totalItems = await donHangQuery.CountAsync();
            viewModel.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (viewModel.CurrentPage < 1) viewModel.CurrentPage = 1;
            if (viewModel.CurrentPage > viewModel.TotalPages && viewModel.TotalPages > 0) viewModel.CurrentPage = viewModel.TotalPages;

            var pagedDonHangs = await donHangQuery
                .Skip((viewModel.CurrentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            viewModel.Orders = pagedDonHangs.Select(dh => new OrderViewModel // Đảm bảo OrderViewModel có đủ các trường
            {
                M_DonHang = dh.M_DonHang,
                CustomerName = dh.KhachHang?.Ten_KhachHang ?? "N/A",
                OrderDate = dh.NgayDat,
                TotalAmount = (decimal)dh.TotalPrice,
                OrderStatusText = dh.TrangThai, // Trạng thái đơn hàng
                OrderStatusBadgeClass = GetOrderStatusBadgeClass(dh.TrangThai),
                PaymentMethod = dh.PhuongThucThanhToan?.TenPhuongThuc ?? "N/A", // Tên phương thức thanh toán
                PaymentStatusText = string.IsNullOrEmpty(dh.TrangThaiThanhToan) ? InferPaymentStatusText(dh) : dh.TrangThaiThanhToan, // Trạng thái thanh toán
                PaymentStatusBadgeClass = GetPaymentStatusBadgeClass(string.IsNullOrEmpty(dh.TrangThaiThanhToan) ? InferPaymentStatusText(dh) : dh.TrangThaiThanhToan)
            }).ToList();

            return View(viewModel); // Sẽ tạo View QuanLyThanhToan.cshtml
        }

        private string GetPaymentStatusBadgeClass(string paymentStatus)
        {
            return paymentStatus switch
            {
                "Chưa thanh toán" => "badge bg-warning text-dark",
                "Chờ TT" => "badge bg-warning text-dark", // Từ hàm Infer
                "Đã thanh toán" => "badge bg-success",
                "Đã TT" => "badge bg-success", // Từ hàm Infer
                "Đã hoàn tiền" => "badge bg-secondary",
                "Thanh toán lỗi" => "badge bg-danger",
                _ => "badge bg-light text-dark",
            };
        }

        private string GetOrderStatusBadgeClass(string dbStatus)
        {
            if (dbStatus == StatusPendingDbValue || dbStatus == "Chưa xử lý") return "badge bg-warning text-dark";
            if (dbStatus == StatusConfirmedDbValue) return "badge bg-info text-dark";
            if (dbStatus == StatusProcessingDbValue) return "badge bg-primary"; // Màu cho "Đang xử lý"
            if (dbStatus == StatusShippingDbValue) return "badge bg-primary"; // Giữ màu cho "Đang giao"
            if (dbStatus == StatusCompletedDbValue) return "badge bg-success";
            if (dbStatus == StatusCancelledDbValue) return "badge bg-danger";
            return "badge bg-secondary";
        }

        private string MapFilterToDbOrderStatus(string filterStatus)
        {
            return filterStatus?.ToLower() switch
            {
                "pending" => StatusPendingDbValue, // Hoặc "Chưa xử lý" nếu đó là giá trị DB
                "confirmed" => StatusConfirmedDbValue,
                "processing" => StatusProcessingDbValue, // Thêm mapping
                "shipping" => StatusShippingDbValue,
                "completed" => StatusCompletedDbValue,
                "cancelled" => StatusCancelledDbValue,
                _ => string.Empty,
            };
        }

        private List<SelectListItem> GetPaymentStatusOptions(string selectedValue)
        {
            // Lấy từ code bạn đã cung cấp hoặc tùy chỉnh
            return new List<SelectListItem> {
                new SelectListItem { Value = "", Text = "Trạng thái TT", Selected = string.IsNullOrEmpty(selectedValue) },
                new SelectListItem { Value = "Chưa thanh toán", Text = "Chưa thanh toán", Selected = selectedValue == "Chưa thanh toán" },
                new SelectListItem { Value = "Đã thanh toán", Text = "Đã thanh toán", Selected = selectedValue == "Đã thanh toán" },
                new SelectListItem { Value = "Đã hoàn tiền", Text = "Đã hoàn tiền", Selected = selectedValue == "Đã hoàn tiền" },
                new SelectListItem { Value = "Thanh toán lỗi", Text = "Thanh toán lỗi", Selected = selectedValue == "Thanh toán lỗi" }
            };
        }

        public async Task<IActionResult> UpdatePaymentStatus(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null) return NotFound();

            ViewBag.M_DonHang = id;
            // Sử dụng hàm GetPaymentStatusOptions đã có trong controller của bạn hoặc tạo mới nếu cần
            ViewBag.PaymentStatusOptions = GetPaymentStatusOptions(donHang.TrangThaiThanhToan); // Giả sử bạn có hàm này
            ViewData["Title"] = $"Cập nhật TT thanh toán đơn hàng {id}";
            // Cần tạo view Areas/QuanLyDH/Views/QuanLyDH/UpdatePaymentStatus.cshtml
            // Trong view đó sẽ có dropdown để chọn newPaymentStatus và POST về action UpdatePaymentStatus (POST)
            return View(); // Trả về view cho phép chọn trạng thái thanh toán mới
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaymentStatus(string id, string newPaymentStatus)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(newPaymentStatus))
            {
                return BadRequest("Thông tin không hợp lệ.");
            }

            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null)
            {
                return NotFound($"Không tìm thấy đơn hàng {id}.");
            }

            // TODO: Validate newPaymentStatus có nằm trong danh sách các trạng thái hợp lệ không.
            // Ví dụ: List<string> validPaymentStatuses = new List<string> { "Chưa thanh toán", "Đã thanh toán", "Đã hoàn tiền", "Thanh toán lỗi"};
            // if (!validPaymentStatuses.Contains(newPaymentStatus)) {
            //      ModelState.AddModelError("newPaymentStatus", "Trạng thái thanh toán không hợp lệ.");
            //      ViewBag.M_DonHang = id;
            //      ViewBag.PaymentStatusOptions = GetPaymentStatusOptions(donHang.TrangThaiThanhToan);
            //      return View( /* ViewModel nếu có */);
            // }


            if (donHang.TrangThaiThanhToan != newPaymentStatus)
            {
                donHang.TrangThaiThanhToan = newPaymentStatus;
                // donHang.NgayCapNhatThanhToan = DateTime.Now; // Nếu có trường này
                _context.Update(donHang);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Trạng thái thanh toán của đơn hàng {donHang.M_DonHang} đã được cập nhật thành '{newPaymentStatus}'.";
            }
            else
            {
                TempData["InfoMessage"] = $"Trạng thái thanh toán của đơn hàng {donHang.M_DonHang} đã là '{newPaymentStatus}'.";
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        private string InferPaymentStatusText(DonHang dh)
        {
            // Logic suy luận rất cơ bản - bạn cần tùy chỉnh cho phù hợp
            if (!string.IsNullOrEmpty(dh.TrangThaiThanhToan)) return dh.TrangThaiThanhToan; // Ưu tiên trạng thái TT đã có

            if (dh.TrangThai == StatusCancelledDbValue) return "Đã hoàn tiền"; // Giả định hủy đơn là hoàn tiền
            if (dh.TrangThai == StatusPendingDbValue || dh.TrangThai == "Chưa xử lý") return "Chưa thanh toán"; // Hoặc "Chờ TT"
            if (dh.PhuongThucThanhToan?.TenPhuongThuc?.ToUpper() == "COD")
            {
                if (dh.TrangThai == StatusCompletedDbValue) return "Đã thanh toán";
                if (dh.TrangThai == StatusShippingDbValue) return "Chưa thanh toán"; // COD thì khi giao mới TT
            }
            if (dh.TrangThai == StatusConfirmedDbValue ||
                dh.TrangThai == StatusProcessingDbValue ||
                dh.TrangThai == StatusShippingDbValue ||
                dh.TrangThai == StatusCompletedDbValue)
            {
                // Giả sử nếu đã qua "Chờ xác nhận" và không phải COD thì là "Đã TT"
                // Logic này cần rất cẩn thận và phụ thuộc vào PTTT
                return "Đã thanh toán"; // Đây là một giả định lớn
            }
            return "Chưa rõ"; // Mặc định
        }

        private List<SelectListItem> GetOrderStatusOptions(string selectedValue)
        {
            // Thêm "Đang xử lý" nếu bạn muốn lọc theo nó
            return new List<SelectListItem> {
                new SelectListItem { Value = "", Text = "Trạng thái ĐH", Selected = string.IsNullOrEmpty(selectedValue) },
                new SelectListItem { Value = "pending", Text = "Chờ xác nhận", Selected = selectedValue == "pending" }, // "Chưa xử lý"
                new SelectListItem { Value = "confirmed", Text = "Đã xác nhận", Selected = selectedValue == "confirmed" },
                new SelectListItem { Value = "processing", Text = "Đang xử lý", Selected = selectedValue == "processing" },
                new SelectListItem { Value = "shipping", Text = "Đang giao", Selected = selectedValue == "shipping" },
                new SelectListItem { Value = "completed", Text = "Hoàn thành", Selected = selectedValue == "completed" },
                new SelectListItem { Value = "cancelled", Text = "Đã hủy", Selected = selectedValue == "cancelled" }
            };
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(string id) // Chuyển thành POST
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null) return NotFound();

            // Kiểm tra điều kiện trước khi xác nhận (ví dụ: không xác nhận nếu đã hủy)
            if (donHang.TrangThai == StatusCancelledDbValue)
            {
                TempData["ErrorMessage"] = $"Đơn hàng {donHang.M_DonHang} đã bị hủy, không thể xác nhận thanh toán.";
                return RedirectToAction(nameof(Index));
            }

            // Chỉ cập nhật nếu trạng thái hiện tại chưa phải là "Đã thanh toán"
            if (donHang.TrangThaiThanhToan != "Đã thanh toán")
            {
                donHang.TrangThaiThanhToan = "Đã thanh toán";
                // donHang.NgayThanhToan = DateTime.Now; // Nếu bạn có trường này
                _context.Update(donHang);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xác nhận thanh toán cho đơn hàng {donHang.M_DonHang}.";
            }
            else
            {
                TempData["InfoMessage"] = $"Đơn hàng {donHang.M_DonHang} đã được xác nhận thanh toán trước đó.";
            }
            return RedirectToAction(nameof(Index)); // Hoặc RedirectToAction(nameof(Details), new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RefundPayment(string id) // Chuyển thành POST
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang == null) return NotFound();

            // Kiểm tra điều kiện (ví dụ: phải "Đã thanh toán" thì mới hoàn tiền)
            if (donHang.TrangThaiThanhToan != "Đã thanh toán")
            {
                TempData["ErrorMessage"] = $"Đơn hàng {donHang.M_DonHang} chưa được thanh toán hoặc đã hoàn tiền, không thể thực hiện hoàn tiền.";
                return RedirectToAction(nameof(Index));
            }

            donHang.TrangThaiThanhToan = "Đã hoàn tiền";
            // donHang.NgayHoanTien = DateTime.Now; // Nếu bạn có trường này
            // TODO: Nếu là hoàn tiền thực sự qua cổng thanh toán, bạn cần thêm logic gọi API của cổng TT ở đây.
            _context.Update(donHang);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã thực hiện hoàn tiền cho đơn hàng {donHang.M_DonHang}.";
            return RedirectToAction(nameof(Index)); // Hoặc RedirectToAction(nameof(Details), new { id = id });
        }

        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var donHang = await _context.DonHangs
                .Include(d => d.KhachHang)
                .Include(d => d.PhuongThucThanhToan)
                .Include(d => d.VanChuyen)
                .Include(d => d.ChiTietDatHangs).ThenInclude(ctdh => ctdh.SanPham)
                .FirstOrDefaultAsync(m => m.M_DonHang == id);
            if (donHang == null) return NotFound();
            ViewData["Title"] = $"Chi tiết đơn hàng {donHang.M_DonHang}";
            ViewData["OrderStatusBadgeClass"] = GetOrderStatusBadgeClass(donHang.TrangThai);

            // Cập nhật để lấy TrangThaiThanhToan trực tiếp nếu có, hoặc suy luận
            ViewData["PaymentStatusText"] = string.IsNullOrEmpty(donHang.TrangThaiThanhToan) ? InferPaymentStatusText(donHang) : donHang.TrangThaiThanhToan;
            ViewData["PaymentStatusBadgeClass"] = GetPaymentStatusBadgeClass(ViewData["PaymentStatusText"].ToString());

            return View(donHang);
        }
    }
   }
