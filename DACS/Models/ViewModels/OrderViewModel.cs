// DACS/Models/ViewModels/OrderManagementViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering; // Namespace này chứa SelectListItem
using System.Collections.Generic;
using System;

namespace DACS.Models.ViewModels
{
    // OrderSummaryItemViewModel giữ nguyên như trước, nhưng sẽ không có TrangThaiThanhToan riêng biệt
    public class OrderSummaryItemViewModel
    {
        public string M_DonHang { get; set; }
        public string TenKhachHang { get; set; }
        public string SoDienThoaiKhachHang { get; set; }
        public DateTime NgayDat { get; set; }
        public float TotalPrice { get; set; }
        public string TrangThaiDonHang { get; set; } // Từ DonHang.TrangThai
        public string BadgeClassDH { get; set; }
        public string TenPhuongThucThanhToan { get; set; }
    }

    public class OrderManagementViewModel
    {
        // Các thẻ thống kê (stats)
        public int OrdersPendingConfirmation { get; set; }
        public int OrdersProcessingOrShipping { get; set; }
        public int OrdersCompletedThisMonth { get; set; }
        public int OrdersCancelledThisMonth { get; set; }

        // Thông tin cho bộ lọc (filters)
        public string SearchString { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string SelectedOrderStatus { get; set; } // Giá trị trạng thái đơn hàng được chọn để lọc

        // Danh sách các lựa chọn cho dropdown trạng thái đơn hàng
        // List<SelectListItem> dùng để tạo <select> trong HTML với các <option>
        public List<SelectListItem> OrderStatusOptions { get; set; }


        // Danh sách đơn hàng và thông tin phân trang
        public List<OrderSummaryItemViewModel> Orders { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public OrderManagementViewModel()
        {
            Orders = new List<OrderSummaryItemViewModel>();
            OrderStatusOptions = new List<SelectListItem>(); // Khởi tạo để tránh lỗi null
        }
    }
}