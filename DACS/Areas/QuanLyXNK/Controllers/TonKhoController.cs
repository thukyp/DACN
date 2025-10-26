// File: Areas/QuanLyXNK/Controllers/TonKhoController.cs
using DACS.Extention;
using DACS.Models;
using DACS.Models.ViewModels;
using DACS.Repositories; // Namespace của Repository
using DACS.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourNameSpace.Extensions;

namespace DACS.Areas.QuanLyXNK.Controllers
{
    [Area("QuanLyXNK")]
    [Authorize(Roles = "Owner,QuanLyXNK")] // Phân quyền tương tự
    public class TonKhoController : Controller
    {
        private readonly ITonKhoRepository _tonKhoRepo; // Sử dụng Interface
        private readonly ILogger<TonKhoController> _logger;
        private readonly ApplicationDbContext _context; // Vẫn giữ context nếu cần truy cập trực tiếp bảng khác

        public TonKhoController(ITonKhoRepository tonKhoRepo, // Inject Interface
                                ILogger<TonKhoController> logger,
                                ApplicationDbContext context)
        {
            _tonKhoRepo = tonKhoRepo;
            _logger = logger;
            _context = context;
        }

        // GET: QuanLyXNK/TonKho
        public async Task<IActionResult> Index(string? searchTerm, string? maKhoFilter, int page = 1)
        {
            
            int pageSize = 15; // Số lượng mục trên mỗi trang (có thể điều chỉnh)
            _logger.LogInformation("Vào TonKho Index - Page: {PageIndex}, Search: '{SearchTerm}', Kho: '{MaKhoFilter}'", page, searchTerm, maKhoFilter);

            try
            {
                // Lấy dữ liệu từ Repository
                var (tonKhoData, totalItems) = await _tonKhoRepo.GetPagedTonKhoAsync(searchTerm, maKhoFilter, page, pageSize);

                // Tính toán số trang
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                // Lấy options cho dropdown kho hàng
                var khoHangOptions = await _tonKhoRepo.GetKhoHangOptionsAsync();

                // --- Tạo Danh sách ViewModel ---
                var listViewModel = new List<TonKhoListItemViewModel>();
                foreach (var tk in tonKhoData)
                {
                    // **QUAN TRỌNG**: Lấy định mức tối thiểu. Logic này cần được hoàn thiện
                    // khi bạn thêm trường DinhMucToiThieu vào model TonKho hoặc bảng khác.
                    long? dinhMuc = null; // Placeholder - Cần lấy giá trị thực
                    // Ví dụ nếu lấy từ bảng LoaiSanPham (cần include trong repo hoặc query riêng):
                    // dinhMuc = tk.LoaiSanPham?.DinhMucTonKhoToiThieu;

                    listViewModel.Add(MapToListItemViewModel(tk, dinhMuc));
                }


                // --- Tạo ViewModel chính cho View ---
                var viewModel = new TonKhoIndexViewModel
                {
                    TonKhoItems = listViewModel,
                    SearchTerm = searchTerm,
                    MaKhoFilter = maKhoFilter,
                    KhoHangOptions = khoHangOptions.ToList(),
                    PageIndex = page,
                    TotalPages = totalPages
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra khi lấy danh sách tồn kho.");
                // Có thể hiển thị trang lỗi hoặc thông báo lỗi thân thiện
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải dữ liệu tồn kho.";
                // Trả về view rỗng hoặc trang lỗi
                return View(new TonKhoIndexViewModel()); // Trả về ViewModel rỗng để tránh lỗi null reference trong View
            }
        }

        // --- Hàm Helper để Map TonKho sang TonKhoListItemViewModel ---
        private TonKhoListItemViewModel MapToListItemViewModel(TonKho tk, long? dinhMucToiThieu)
        {
            var khoiLuongDonHang = _context.ChiTietDatHangs
    .Where(ct => ct.ProductId == tk.M_SanPham
                 && ct.DonHang.TrangThai == "Đã xác nhận")
    .Sum(ct => ct.Khoiluong);

            var khoiLuongConLai = tk.KhoiLuong - khoiLuongDonHang;
            

            return new TonKhoListItemViewModel
            {
                Id = tk.Id,
                TenKho = tk.KhoHang?.TenKho ?? tk.MaKho,
                TenSanPham = tk.SanPham?.TenSanPham ?? tk.M_SanPham,
                TenDonViTinh = tk.DonViTinh?.TenLoaiTinh ?? tk.M_DonViTinh,
                KhoiLuong = khoiLuongConLai,
                DinhMucToiThieu = dinhMucToiThieu,
                TrangThai = CalculateStatus(tk.KhoiLuong, dinhMucToiThieu)
            };
            
        }
       




        // --- Hàm Helper để tính toán Trạng thái Tồn kho ---
        private string CalculateStatus(float currentQuantity, long? minThreshold)
        {
            if (currentQuantity <= 0)
            {
                return "Hết hàng"; // Badge màu đỏ (danger)
            }
            if (minThreshold.HasValue) // Chỉ xét nếu có định mức
            {
                if (currentQuantity < minThreshold.Value)
                {
                    return "Sắp hết"; // Badge màu vàng (warning)
                }
                else
                {
                    return "Đủ hàng"; // Badge màu xanh lá (success)
                }
            }
            else // Nếu không có định mức, chỉ báo là còn hàng
            {
                return "Còn hàng"; // Badge màu xám (secondary) hoặc xanh lá (success)
            }
        }
    }
}