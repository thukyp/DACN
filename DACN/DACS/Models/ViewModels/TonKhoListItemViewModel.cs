using System.ComponentModel.DataAnnotations;

namespace DACS.Models.ViewModels
{
    public class TonKhoListItemViewModel
    {
        public int Id { get; set; } // Giữ lại ID để có thể cần cho các action sau này

        [Display(Name = "Kho hàng")]
        public string TenKho { get; set; } = "N/A"; // Mặc định nếu không load được

        [Display(Name = "Sản phẩm")]
        public string TenSanPham { get; set; } = "N/A";

        [Display(Name = "Đơn vị")]
        public string TenDonViTinh { get; set; } = "N/A";

        [Display(Name = "Khối lượng còn")]
        public float KhoiLuong { get; set; }

        // Quan trọng: Trường này KHÔNG có trong Model TonKho bạn cung cấp.
        // Cần thêm trường này vào Model TonKho và cập nhật DB thì logic trạng thái mới chính xác.
        // Tạm thời để nullable để code chạy được.
        [Display(Name = "Định mức tối thiểu")]
        public long? DinhMucToiThieu { get; set; }

        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Không xác định"; // Sẽ được tính toán

        [Display(Name = "Ngày nhập")]
        [DataType(DataType.Date)]
        public DateTime NgayNhapKho { get; set; }

        [Display(Name = "Hạn sử dụng")]
        [DataType(DataType.Date)]
        public DateTime? HanSuDung { get; set; }

        [Display(Name = "Số lô")]
        public string? SoLo { get; set; }
    }
}
