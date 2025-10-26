// File: Areas/Owner/Models/RevenueReportViewModels.cs (hoặc tên tương tự)
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DACS.Areas.Owner.Models
{
    public class DailyRevenueDetailViewModel
    {
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal RevenueAmount { get; set; }
    }

    public class ChartDataPointViewModel
    {
        public string Label { get; set; } // Nhãn (ngày, tên loại...)
        public decimal Value { get; set; } // Giá trị (doanh thu)
    }
    public class PieChartDataPointViewModel // Dành riêng cho biểu đồ tròn với màu sắc
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
        public string Color { get; set; } // Mã màu HTML (vd: "#FF6384")
    }


    public class RevenueReportPageViewModel
    {
        // Filters
        public string SelectedDateRangePreset { get; set; } = "7days"; // Mặc định 7 ngày qua
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? CustomDateFrom { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? CustomDateTo { get; set; }

        public List<SelectListItem> DateRangePresetOptions { get; set; }

        // Calculated effective date range for display and query
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime EffectiveDateFrom { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime EffectiveDateTo { get; set; }
        public string DisplayDateRangeText { get; set; }


        // Statistics Cards
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal AveragePerOrder => TotalOrders > 0 ? TotalRevenue / TotalOrders : 0;
        public double RevenueChangePercentage { get; set; } // So với kỳ trước
        public string RevenueChangeTrend { get; set; } // "up", "down", "neutral" để style icon/màu

        // Chart Data
        public List<string> RevenueOverTimeLabels { get; set; } // Labels cho trục X (ngày, tháng)
        public List<decimal> RevenueOverTimeData { get; set; }   // Dữ liệu cho trục Y (doanh thu)

        public List<string> RevenueByCategoryLabels { get; set; } // Tên các loại phụ phẩm
        public List<decimal> RevenueByCategoryData { get; set; }   // Doanh thu tương ứng
        public List<string> RevenueByCategoryColors { get; set; } // Màu cho từng phần trong biểu đồ tròn

        // Detailed Table Data
        public List<DailyRevenueDetailViewModel> DailyRevenueDetails { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0} đ")]
        public decimal SumTotalDetailedRevenue => DailyRevenueDetails?.Sum(d => d.RevenueAmount) ?? 0;
        public int SumTotalDetailedOrders => DailyRevenueDetails?.Sum(d => d.OrderCount) ?? 0;


        public RevenueReportPageViewModel()
        {
            DateRangePresetOptions = new List<SelectListItem>();
            RevenueOverTimeLabels = new List<string>();
            RevenueOverTimeData = new List<decimal>();
            RevenueByCategoryLabels = new List<string>();
            RevenueByCategoryData = new List<decimal>();
            RevenueByCategoryColors = new List<string>();
            DailyRevenueDetails = new List<DailyRevenueDetailViewModel>();
        }
    }
}