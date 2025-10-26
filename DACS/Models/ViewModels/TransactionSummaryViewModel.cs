using System;
using System.Collections.Generic;

namespace DACS.Models.ViewModels // Đảm bảo đúng namespace
{
    // ViewModel cho một dòng lịch sử giao dịch
    public class TransactionSummaryViewModel
    {
        public string TransactionId { get; set; } // Mã GD
        public string? RelatedOrderId { get; set; } // Mã Đơn hàng liên quan (nếu có)
        public DateTime TransactionDate { get; set; } // Ngày GD
        public decimal Amount { get; set; } // Số tiền
        public string PaymentMethod { get; set; } // Phương thức
        public string Status { get; set; } // Trạng thái
    }

    // ViewModel cho toàn bộ trang Lịch sử giao dịch
    public class TransactionHistoryViewModel
    {
        public List<TransactionSummaryViewModel> Transactions { get; set; } = new List<TransactionSummaryViewModel>();

        // Thông tin phân trang
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        // Thông tin lọc (để giữ lại giá trị trên form)
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}