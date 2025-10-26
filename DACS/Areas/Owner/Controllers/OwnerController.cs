using System.ComponentModel.DataAnnotations;
using DACS.Models.ViewModels;
using DACS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DACS.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")]
    public class OwnerController : Controller
    {
            private readonly ApplicationDbContext _context;
            private readonly UserManager<ApplicationUser> _userManager;

            public OwnerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
            {
                _context = context;
                _userManager = userManager;
            }

            // GET: Owner/Owner (Dashboard)
            public async Task<IActionResult> Index()
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Challenge(); // Hoặc NotFound("User not found.");
                }

                // Lấy thông tin Owner từ ApplicationUser.Id
                // Trong model Owner.cs của bạn, Id là ForeignKey đến ApplicationUser.Id
                var ownerEntity = await _context.QuanLys
                                        .Include(o => o.User)
                                        .FirstOrDefaultAsync(o => o.UserId == currentUser.Id);

                string ownerName = ownerEntity?.User?.UserName ?? currentUser.FullName ?? currentUser.UserName;

                // --- Tính toán số liệu cho Dashboard ---

                // Doanh thu (Tháng hiện tại)
                var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                decimal monthlyRevenue = (decimal)await _context.DonHangs
                    .Where(dh => dh.NgayDat.Date >= firstDayOfMonth.Date && dh.NgayDat.Date <= lastDayOfMonth.Date && dh.TrangThai == "Hoàn thành")
                    .SumAsync(dh => dh.TotalPrice);

                // Đơn hàng mới (Tuần hiện tại)
                var today = DateTime.Today;
                // DayOfWeek.Sunday is 0, Monday is 1, etc. Adjust if your week starts on Monday.
                // Assuming week starts on Sunday for this calculation.
                var firstDayOfWeek = today.AddDays(-(int)today.DayOfWeek);
                var lastDayOfWeek = firstDayOfWeek.AddDays(6);
                int newOrdersThisWeek = await _context.DonHangs
                    .CountAsync(dh => dh.NgayDat.Date >= firstDayOfWeek.Date && dh.NgayDat.Date <= lastDayOfWeek.Date);
                // Thêm filter theo Owner/Shop nếu cần

                // Người dùng mới (Tháng)
                // LƯU Ý: Logic này hiện tại đếm tổng số người dùng đang hoạt động (không bị khóa).
                // Để đếm chính xác "người dùng mới trong tháng", ApplicationUser cần có trường Ngày Tạo/Đăng ký.
                int newUsersThisMonth = await _context.Users.CountAsync(u => u.LockoutEnd == null || u.LockoutEnd < DateTimeOffset.UtcNow);
                // Nếu bạn muốn đếm số lượng Khách Hàng mới liên quan đến Owner này trong tháng (cần định nghĩa "Khách Hàng" và mối quan hệ):
                // int newCustomersThisMonth = await _context.KhachHang
                //    .Where(kh => kh.OwnerId == ownerEntity.MaOwner && kh.NgayTao >= firstDayOfMonth) 
                //    .CountAsync();


                // YC Thu gom chờ xử lý
                // Giả sử YeuCauThuGom có liên kết với Owner hoặc sản phẩm của Owner
                int pendingCollectionRequests = await _context.YeuCauThuGoms
                    .CountAsync(yc => yc.TrangThai == "Chờ xử lý");
                // Thêm filter theo Owner/Shop nếu cần (ví dụ: qua MaSanPham, rồi SanPham thuộc Owner)

                // --- Dữ liệu ví dụ cho Biểu đồ Doanh thu (7 ngày gần nhất) ---
                var revenueChartLabels = new List<string>();
                var revenueChartData = new List<decimal>();
                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Today.AddDays(-i);
                    revenueChartLabels.Add(date.ToString("dd/MM"));
                    decimal dailyRevenue = (decimal)await _context.DonHangs
                        .Where(dh => dh.NgayDat.Date == date.Date && dh.TrangThai == "Hoàn thành")
                        // Thêm filter theo Owner/Shop nếu cần
                        .SumAsync(dh => dh.TotalPrice);
                    revenueChartData.Add(dailyRevenue / 1000000); // Ví dụ: Hiển thị theo triệu VND
                }


                var viewModel = new OwnerDashboardViewModel
                {
                    MonthlyRevenue = monthlyRevenue,
                    NewOrdersThisWeek = newOrdersThisWeek,
                    NewUsersThisMonth = newUsersThisMonth, // Cần xem lại logic nếu muốn độ chính xác cao
                    PendingCollectionRequests = pendingCollectionRequests,
                    OwnerName = ownerName,
                    RevenueChartLabels = revenueChartLabels,
                    RevenueChartData = revenueChartData
                };

                return View(viewModel);
            }

    }
    
}
