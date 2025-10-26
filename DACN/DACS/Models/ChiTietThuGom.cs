using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DACS.Models
{
    public class ChiTietThuGom // Collection Detail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // <<< THÊM ATTRIBUTE NÀY
        public String? M_ChiTiet { get;  set; }
        [Required] // M_YeuCau is Required
        [StringLength(10)]
        public string M_YeuCau { get; set; } // PFK

        [ForeignKey("M_YeuCau")]
        public virtual YeuCauThuGom YeuCauThuGom { get; set; }
        [Required]
        [StringLength(10)]
        public string M_DonViTinh { get; set; } // FK

        [Required]
        [StringLength(10)]
        // CẢNH BÁO: Có thể thừa? PK nên là (M_YeuCau, M_SanPham)?
        public string M_KhachHang { get; set; } // PFK

        [Required]
        public int SoLuong { get; set; } // Số lượng thu gom

        [Display(Name = "Cồng Kềnh")]
        public bool DacTinh_CongKenh { get; set; } = false; // Map từ charBulky

        [Display(Name = "Ẩm/Ướt (Dễ hỏng)")]
        public bool DacTinh_AmUot { get; set; } = false; // Map từ charWet

        [Display(Name = "Khô (Dễ cháy)")]
        public bool DacTinh_Kho { get; set; } = false; // Map từ charDry

        [Display(Name = "Độ Ẩm Cao (>20%)")]
        public bool DacTinh_DoAmCao { get; set; } = false; // Map từ charMoisture

        [Display(Name = "Nhiều Tạp Chất")]
        public bool DacTinh_TapChat { get; set; } = false; // Map từ charImpure

        [Display(Name = "Đã Qua Xử Lý")]
        public bool DacTinh_DaXuLy { get; set; } = false; // Map từ charProcessed

        // --- THÊM: Danh sách hình ảnh ---
        [Display(Name = "Hình Ảnh")]
        public string? DanhSachHinhAnh { get; set; }



        // --- Thêm Khóa Ngoại và Navigation Property đến SanPham ---
        // (Cần thêm cột M_SanPham vào bảng ChiTietThuGom trong CSDL của bạn nếu chưa có)
        // --- Hết phần thêm ---

        [ForeignKey("M_DonViTinh")]
        public virtual DonViTinh DonViTinh { get; set; }

        [ForeignKey("M_KhachHang")]
        public virtual KhachHang KhachHang { get; set; }

        // --- CÁC TRƯỜNG MÃ ĐỊA CHỈ LÀM KHÓA NGOẠI ---
        // Kiểu dữ liệu (string/int) và độ dài phải khớp với Khóa chính của bảng ĐVHC
        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành phố.")]
        [StringLength(10)] // Hoặc int
        [Display(Name = "Tỉnh/Thành phố")]
        public string MaTinh { get; set; } // <<< Lưu Mã Tỉnh (FK)

        [Required(ErrorMessage = "Vui lòng chọn Quận/Huyện.")]
        [StringLength(10)] // Hoặc int
        [Display(Name = "Quận/Huyện")]
        public string MaQuan { get; set; } // <<< Lưu Mã Quận (FK)

        [Required(ErrorMessage = "Vui lòng chọn Xã/Phường.")]
        [StringLength(10)] // Hoặc int
        [Display(Name = "Xã/Phường")]
        public string MaXa { get; set; } // <<< Lưu Mã Xã (FK)
                                         // --- KẾT THÚC MÃ ĐỊA CHỈ ---

        // --- Trường địa chỉ chi tiết (Số nhà/Đường) ---
        [StringLength(200)]
        [Display(Name = "Số nhà, Đường/Ấp/Thôn")]
        public string? DiaChi_DuongApThon { get; set; } // Lưu phần còn lại
                                                        // -----------------------------------------

        // --- Navigation Properties đến các bảng địa chỉ ---
        [ForeignKey("MaTinh")]
        public virtual TinhThanhPho? TinhThanhPho { get; set; }

        [ForeignKey("MaQuan")]
        public virtual QuanHuyen? QuanHuyen { get; set; }

        [ForeignKey("MaXa")]
        public virtual XaPhuong? XaPhuong { get; set; }
        public string? MoTa { get;  set; }
        public string M_LoaiSP { get;  set; }
        [ForeignKey("M_LoaiSP")] // Link to the FK property
        public virtual LoaiSanPham LoaiSanPham { get; set; } // Add the navigation property
        public decimal? GiaTriMongMuon { get;  set; }

        // --- Kết thúc Navigation địa chỉ ---
    }

}
