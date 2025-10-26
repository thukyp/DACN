// File: Areas/Owner/Controllers/DoanhThuController.cs
using DACS.Models; // Giả sử DonHang, ChiTietDatHang, SanPham, LoaiSanPham ở đây
using DACS.Areas.Owner.Models; // RevenueReportViewModels
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace DACS.Areas.Owner.Controllers
{
    [Area("Owner")]
    [Authorize(Roles = "Owner")] // Điều chỉnh role nếu cần
    public class DoanhThuController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Định nghĩa chuỗi trạng thái đơn hàng hoàn thành (lấy từ controller quản lý đơn hàng)
        private const string StatusCompletedDbValue = "Hoàn thành"; // PHẢI KHỚP VỚI DB

        public DoanhThuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Owner/DoanhThu
        public async Task<IActionResult> Index(string selectedDateRangePreset = "7days", DateTime? customDateFrom = null, DateTime? customDateTo = null)
        {
            var viewModel = new RevenueReportPageViewModel
            {
                SelectedDateRangePreset = selectedDateRangePreset,
                CustomDateFrom = customDateFrom,
                CustomDateTo = customDateTo,
                DateRangePresetOptions = GetDateRangePresetOptions(selectedDateRangePreset)
            };

            CalculateEffectiveDateRange(viewModel);
            string dateRangeDisplay = $"{viewModel.EffectiveDateFrom:dd/MM/yyyy} - {viewModel.EffectiveDateTo:dd/MM/yyyy}";
            switch (selectedDateRangePreset)
            {
                case "today": dateRangeDisplay = $"Hôm nay ({viewModel.EffectiveDateFrom:dd/MM/yyyy})"; break;
                case "yesterday": dateRangeDisplay = $"Hôm qua ({viewModel.EffectiveDateFrom:dd/MM/yyyy})"; break;
                case "this_month": dateRangeDisplay = $"Tháng này ({viewModel.EffectiveDateFrom:MM/yyyy})"; break;
                case "last_month": dateRangeDisplay = $"Tháng trước ({viewModel.EffectiveDateFrom:MM/yyyy})"; break;
            }
            viewModel.DisplayDateRangeText = dateRangeDisplay;


            // Lấy đơn hàng đã hoàn thành trong khoảng thời gian hiệu lực
            var completedOrdersQuery = _context.DonHangs
                                        .Where(dh => dh.TrangThai == StatusCompletedDbValue &&
                                                     dh.NgayDat.Date >= viewModel.EffectiveDateFrom.Date && // Hoặc NgayHoanThanh
                                                     dh.NgayDat.Date <= viewModel.EffectiveDateTo.Date);    // Hoặc NgayHoanThanh

            var ordersInPeriod = await completedOrdersQuery.ToListAsync();

            viewModel.TotalRevenue = ordersInPeriod.Sum(dh => (decimal)dh.TotalPrice); // Ép kiểu float sang decimal
            viewModel.TotalOrders = ordersInPeriod.Count();

            // Tính toán "So với kỳ trước"
            // (Cần logic xác định kỳ trước dựa trên effectiveDateFrom và effectiveDateTo)
            // Ví dụ đơn giản: kỳ trước có cùng độ dài thời gian
            TimeSpan periodDuration = viewModel.EffectiveDateTo.Date - viewModel.EffectiveDateFrom.Date;
            DateTime previousPeriodEnd = viewModel.EffectiveDateFrom.Date.AddDays(-1);
            DateTime previousPeriodStart = previousPeriodEnd.Date.AddDays(-periodDuration.Days);

            var ordersInPreviousPeriod = await _context.DonHangs
                                        .Where(dh => dh.TrangThai == StatusCompletedDbValue &&
                                                     dh.NgayDat.Date >= previousPeriodStart.Date &&
                                                     dh.NgayDat.Date <= previousPeriodEnd.Date)
                                        .ToListAsync();
            decimal totalRevenuePreviousPeriod = ordersInPreviousPeriod.Sum(dh => (decimal)dh.TotalPrice);

            if (totalRevenuePreviousPeriod > 0)
            {
                viewModel.RevenueChangePercentage = Math.Round(((double)viewModel.TotalRevenue - (double)totalRevenuePreviousPeriod) / (double)totalRevenuePreviousPeriod * 100, 2);
            }
            else if (viewModel.TotalRevenue > 0)
            {
                viewModel.RevenueChangePercentage = 100; // Kỳ trước = 0, kỳ này > 0
            }
            else
            {
                viewModel.RevenueChangePercentage = 0;
            }
            viewModel.RevenueChangeTrend = viewModel.RevenueChangePercentage > 0 ? "up" : (viewModel.RevenueChangePercentage < 0 ? "down" : "neutral");


            // Dữ liệu cho biểu đồ "Doanh thu theo thời gian"
            var revenueOverTimeData = ordersInPeriod
                .GroupBy(dh => dh.NgayDat.Date) // Hoặc NgayHoanThanh.Date
                .Select(g => new ChartDataPointViewModel { Label = g.Key.ToString("dd/MM"), Value = g.Sum(dh => (decimal)dh.TotalPrice) })
                .OrderBy(x => DateTime.ParseExact(x.Label, "dd/MM", CultureInfo.InvariantCulture)) // Sắp xếp lại theo ngày cho đúng
                .ToList();

            viewModel.RevenueOverTimeLabels = revenueOverTimeData.Select(d => d.Label).ToList();
            viewModel.RevenueOverTimeData = revenueOverTimeData.Select(d => d.Value).ToList();


            // Dữ liệu cho biểu đồ "Doanh thu theo loại phụ phẩm"
            // Cần Include ChiTietDatHangs -> SanPham -> LoaiSanPham (hoặc thuộc tính loại trên SanPham)
            var revenueByCategoryData = await _context.DonHangs
                .Where(dh => dh.TrangThai == StatusCompletedDbValue &&
                             dh.NgayDat.Date >= viewModel.EffectiveDateFrom.Date &&
                             dh.NgayDat.Date <= viewModel.EffectiveDateTo.Date)
                .Include(dh => dh.ChiTietDatHangs)
                    .ThenInclude(ctdh => ctdh.SanPham) // Giả sử SanPham có TenLoaiSanPham hoặc liên kết đến LoaiSanPham
                                                       // .ThenInclude(sp => sp.LoaiSanPham) // Nếu LoaiSanPham là entity riêng
                .SelectMany(dh => dh.ChiTietDatHangs)
                .GroupBy(ctdh => ctdh.SanPham.TenSanPham) // HOẶC ctdh.SanPham.LoaiSanPham.TenLoai, hoặc ctdh.SanPham.TenLoaiSanPham
                .Select(g => new ChartDataPointViewModel
                {
                    Label = g.Key ?? "Chưa phân loại", // Tên loại sản phẩm/phụ phẩm
                    Value = g.Sum(ctdh => ctdh.TongTien) // Giả sử ChiTietDatHang có ThanhTien = SoLuong * DonGia
                })
                .OrderByDescending(x => x.Value)
                .Take(5) // Lấy 5 loại có doanh thu cao nhất + gộp phần còn lại vào "Khác" (logic phức tạp hơn)
                .ToListAsync();

            viewModel.RevenueByCategoryLabels = revenueByCategoryData.Select(d => d.Label).ToList();
            viewModel.RevenueByCategoryData = revenueByCategoryData.Select(d => d.Value).ToList();
            // Tạo màu ngẫu nhiên hoặc dùng danh sách màu định sẵn cho biểu đồ tròn
            var random = new Random();
            foreach (var _ in viewModel.RevenueByCategoryLabels)
            {
                viewModel.RevenueByCategoryColors.Add($"rgba({random.Next(256)}, {random.Next(256)}, {random.Next(256)}, 0.7)");
            }


            // Dữ liệu cho bảng "Doanh thu chi tiết theo ngày"
            viewModel.DailyRevenueDetails = ordersInPeriod
                .GroupBy(dh => dh.NgayDat.Date) // Hoặc NgayHoanThanh.Date
                .Select(g => new DailyRevenueDetailViewModel
                {
                    Date = g.Key,
                    OrderCount = g.Count(),
                    RevenueAmount = g.Sum(dh => (decimal)dh.TotalPrice)
                })
                .OrderByDescending(d => d.Date)
                .ToList();

            return View(viewModel);
        }

        private void CalculateEffectiveDateRange(RevenueReportPageViewModel viewModel)
        {
            DateTime today = DateTime.Now.Date; // Current date in Vietnam (server time)
            // Dựa trên múi giờ của server. Nếu cần múi giờ cụ thể, cần TimeZoneInfo.ConvertTime.
            // For simplicity, using server's current notion of "today".

            switch (viewModel.SelectedDateRangePreset)
            {
                case "today":
                    viewModel.EffectiveDateFrom = today;
                    viewModel.EffectiveDateTo = today;
                    break;
                case "yesterday":
                    viewModel.EffectiveDateFrom = today.AddDays(-1);
                    viewModel.EffectiveDateTo = today.AddDays(-1);
                    break;
                case "7days":
                    viewModel.EffectiveDateFrom = today.AddDays(-6);
                    viewModel.EffectiveDateTo = today;
                    break;
                case "30days":
                    viewModel.EffectiveDateFrom = today.AddDays(-29);
                    viewModel.EffectiveDateTo = today;
                    break;
                case "this_month":
                    viewModel.EffectiveDateFrom = new DateTime(today.Year, today.Month, 1);
                    viewModel.EffectiveDateTo = viewModel.EffectiveDateFrom.AddMonths(1).AddDays(-1);
                    if (viewModel.EffectiveDateTo > today) viewModel.EffectiveDateTo = today; // Đảm bảo không vượt quá ngày hiện tại
                    break;
                case "last_month":
                    var firstDayOfThisMonth = new DateTime(today.Year, today.Month, 1);
                    viewModel.EffectiveDateFrom = firstDayOfThisMonth.AddMonths(-1);
                    viewModel.EffectiveDateTo = firstDayOfThisMonth.AddDays(-1);
                    break;
                case "custom":
                    viewModel.EffectiveDateFrom = viewModel.CustomDateFrom ?? today.AddDays(-6); // Mặc định 7 ngày nếu custom rỗng
                    viewModel.EffectiveDateTo = viewModel.CustomDateTo ?? today;
                    if (viewModel.EffectiveDateFrom > viewModel.EffectiveDateTo) // Đảo ngược nếu from > to
                    {
                        var temp = viewModel.EffectiveDateFrom;
                        viewModel.EffectiveDateFrom = viewModel.EffectiveDateTo;
                        viewModel.EffectiveDateTo = temp;
                    }
                    break;
                default: // Mặc định là 7 ngày qua
                    viewModel.EffectiveDateFrom = today.AddDays(-6);
                    viewModel.EffectiveDateTo = today;
                    break;
            }
        }

        private List<SelectListItem> GetDateRangePresetOptions(string selectedValue)
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "today", Text = "Hôm nay", Selected = selectedValue == "today" },
                new SelectListItem { Value = "yesterday", Text = "Hôm qua", Selected = selectedValue == "yesterday" },
                new SelectListItem { Value = "7days", Text = "7 ngày qua", Selected = selectedValue == "7days" },
                new SelectListItem { Value = "30days", Text = "30 ngày qua", Selected = selectedValue == "30days" },
                new SelectListItem { Value = "this_month", Text = "Tháng này", Selected = selectedValue == "this_month" },
                new SelectListItem { Value = "last_month", Text = "Tháng trước", Selected = selectedValue == "last_month" },
                new SelectListItem { Value = "custom", Text = "Tùy chỉnh...", Selected = selectedValue == "custom" }
            };
        }

        // TODO: Action for ExportReport
        // public async Task<IActionResult> ExportReport(string selectedDateRangePreset, DateTime? customDateFrom, DateTime? customDateTo)
        // {
        //     // Logic tương tự Index để lấy dữ liệu
        //     // Rồi dùng thư viện (EPPlus, CsvHelper) để tạo file Excel/CSV
        //     // return File(...);
        // }
    }
}