using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần cho SelectList

namespace DACS.Models.ViewModels // Đảm bảo đúng namespace
{
    // ViewModel để hiển thị một dòng trong lịch sử đơn hàng
    public class OrderSummaryViewModel
    {
        public string OrderId { get; set; } // Ví dụ: M_CTDatHang
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public int KhoiLuong { get;  set; }
        // Thêm các trường khác nếu cần hiển thị (ví dụ: tên sản phẩm đầu tiên...)
    }

    // ViewModel cho toàn bộ trang Lịch sử đơn hàng
    public class OrderHistoryViewModel
    {
        public List<OrderSummaryViewModel> Orders { get; set; } = new List<OrderSummaryViewModel>();

        // Thông tin phân trang
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        // Thông tin lọc (để giữ lại giá trị trên form)
        public string CurrentStatusFilter { get; set; }
        public string CurrentTimeFilter { get; set; }

        // Tùy chọn: Danh sách cho dropdown lọc (có thể tạo ở Controller hoặc để tĩnh)
        // public SelectList StatusFilterOptions { get; set; }
        // public SelectList TimeFilterOptions { get; set; }
    }
}