using DACS.Models; // Nơi chứa Model 'LoHang' của bạn
using DACS.Models.Blockchain; // Nơi chứa Model 'TraceEventDTO'
using System.Collections.Generic;

namespace DACS.Models.ViewModels
{
    public class NhatKyViewModel
    {
        // Thông tin chung của lô, lấy từ SQL
        public LoTonKho  LotInfo { get; set; }

        // Lịch sử nhật ký, lấy từ Blockchain
        public List<TraceEventDTO> History { get; set; }

        // Có thể thêm các thuộc tính khác nếu bạn muốn
        // public SanPham ProductInfo { get; set; } // Ví dụ nếu bạn muốn join thêm bảng Sản Phẩm
    }
}