// File: Areas/Owner/Models/TransactionViewModels.cs (create or add to existing)
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace DACS.Areas.Owner.Models
{
    public class TransactionViewModel // For each row in the transaction table
    {
        public string TransactionId { get; set; }
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }

        public string PaymentMethodDisplay { get; set; } // e.g., "<i class='fab fa-cc-visa text-primary'></i> Visa **** 1234"
        public string PaymentMethodRaw { get; set; } // e.g., "Visa", "MoMo" (for filtering or logic)

        public string StatusText { get; set; }
        public string StatusBadgeClass { get; set; }
    }

    public class TransactionManagementViewModel // For the entire page
    {
        // Statistics
        public decimal TotalTransactionAmountThisMonth { get; set; }
        public int SuccessfulTransactionsCount { get; set; } // Count for "Giao dịch thành công"
        public int FailedTransactionsCount { get; set; }     // Count for "Giao dịch thất bại"
        public int RefundedTransactionsCount { get; set; }   // Count for "Đã hoàn tiền"

        // Filters
        public string SearchInput { get; set; }
        public DateTime? DateFromFilter { get; set; }
        public DateTime? DateToFilter { get; set; }
        public string SelectedTransactionStatus { get; set; } // e.g., "pending", "successful"
        public string SelectedPaymentMethod { get; set; }   // e.g., "cod", "bank"

        // Options for filter dropdowns
        public List<SelectListItem> TransactionStatusOptions { get; set; }
        public List<SelectListItem> PaymentMethodOptions { get; set; }

        // Table Data & Pagination
        public List<TransactionViewModel> Transactions { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages && TotalPages > 0;

        public TransactionManagementViewModel()
        {
            Transactions = new List<TransactionViewModel>();
            TransactionStatusOptions = new List<SelectListItem>();
            PaymentMethodOptions = new List<SelectListItem>();
        }
    }
}