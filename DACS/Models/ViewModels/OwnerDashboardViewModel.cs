using System.Collections.Generic; // Cần thiết để sử dụng List<T>

namespace DACS.Models.ViewModels // Đảm bảo namespace này khớp với cấu trúc dự án của bạn
{
    public class OwnerDashboardViewModel
    {
        // Các số liệu thống kê chính
        public decimal MonthlyRevenue { get; set; }
        public int NewOrdersThisWeek { get; set; }
        public int NewUsersThisMonth { get; set; } // Như đã thảo luận, logic tính toán cho mục này có thể cần xem lại để đảm bảo độ chính xác
        public int PendingCollectionRequests { get; set; }
        public string OwnerName { get; set; } // Tên của Owner để hiển thị lời chào

        // Dữ liệu cho biểu đồ doanh thu
        public List<string> RevenueChartLabels { get; set; }
        public List<decimal> RevenueChartData { get; set; }

        // Constructor để khởi tạo các List, tránh lỗi NullReferenceException
        public OwnerDashboardViewModel()
        {
            RevenueChartLabels = new List<string>();
            RevenueChartData = new List<decimal>();
        }
    }
}