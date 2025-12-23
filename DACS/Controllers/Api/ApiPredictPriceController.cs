using DACS.Models;
using DACS.Models.AI;
using DACS.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DACS.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiPredictPriceController : ControllerBase

    {
        private readonly ILogger<ThuGomController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public ApiPredictPriceController(
            IConfiguration configuration,
            ILogger<ThuGomController> logger,
            ApplicationDbContext dbContext,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _configuration = configuration;
            _logger = logger;
            _context = dbContext;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _emailService = emailService;
        }
        public async Task<IActionResult> PredictPrice([FromBody] PriceRequest req)
        {

            var sanPham = _context.SanPhams.FirstOrDefault(p => p.M_SanPham == req.NhomSanPham);

            if (sanPham == null)
                return BadRequest("Không tìm thấy sản phẩm trong hệ thống.");

            double heSoMuaVu = CheckSeasonalFactor(sanPham);

            string fullAddress = $" {req.Ward}, {req.District}, {req.Province}, Việt Nam";
            var (userLat, userLng) = await GetCoordinatesAsync(fullAddress);

            if (userLat == 0 && userLng == 0)
                return BadRequest("Không tìm thấy địa chỉ.");

            var khoList = _context.KhoHangs.ToList();
            KhoHang khoGanNhat = null;
            double minDistance = double.MaxValue;

            foreach (var k in khoList)
            {
                double d = DistanceKm(userLat, userLng, k.Lat, k.Lng);
                if (d < minDistance)
                {
                    minDistance = d;
                    khoGanNhat = k;
                }
            }
            double roadDistanceKm = minDistance * 1.3;
            double shippingFee = CalculateShipping(roadDistanceKm, req.KhoiLuong);

            double doAmChuan = 15.0;
            double chenhLechDoAm = req.DoAmThucTe - doAmChuan;
            double heSoDoAm = 1.0;

            if (chenhLechDoAm > 0)
                heSoDoAm = 1.0 - (chenhLechDoAm * 0.012);
            else
                heSoDoAm = 1.0 + (Math.Abs(chenhLechDoAm) * 0.005);

            if (heSoDoAm < 0.6) heSoDoAm = 0.6;
            double HE_SO_BIEN_LOI_NHUAN = 0.8;

            double giaCoSoThuMua = (double)sanPham.Gia * HE_SO_BIEN_LOI_NHUAN;
            double donGiaUocTinh = giaCoSoThuMua * heSoMuaVu * heSoDoAm;

            double tongGiaTriHang = donGiaUocTinh * req.KhoiLuong;

            double giaCuoiCung = Math.Max(0, tongGiaTriHang - shippingFee);

            return Ok(new
            {
                TenSanPham = sanPham.TenSanPham ?? "Tên lỗi",
                KhoGanNhat = khoGanNhat?.TenKho ?? "Chưa tìm thấy kho",
                QuangDuong = Math.Round(roadDistanceKm, 1),

                GiaGocTaiKho = (double)sanPham.Gia,
                HeSoMuaVu = heSoMuaVu,
                HeSoDoAm = Math.Round(heSoDoAm, 2),

                PhiVanChuyen = Math.Round(shippingFee),

                TotalPrice = Math.Round(giaCuoiCung / 1000) * 1000
            });
        }
        //tính km khoảng cách
        private double DistanceKm(double lat1, double lng1, double lat2, double lng2)
        {
            var R = 6371;
            var dLat = ToRad(lat2 - lat1);
            var dLng = ToRad(lng2 - lng1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
        private double ToRad(double deg) => deg * Math.PI / 180;
        //lấy địa chỉ lat log
        private async Task<(double lat, double lng)> GetCoordinatesAsync(string address)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "DACS_NongNghiep/1.0 (contact@email.com)");
                client.Timeout = TimeSpan.FromSeconds(10);


                var url = $"https://nominatim.openstreetmap.org/search?format=json&limit=1&q={Uri.EscapeDataString(address)}";
                var response = await client.GetStringAsync(url);
                dynamic data = JsonConvert.DeserializeObject(response);

                if (data.Count == 0) return (0, 0);
                return ((double)data[0].lat, (double)data[0].lon);
            }
            catch
            {
                return (0, 0);
            }
        }
        //check season
        private double CheckSeasonalFactor(SanPham sp)
        {
            int currentMonth = DateTime.Now.Month;
            bool isInSeason = false;
            if (sp.ThangBatDauVu <= sp.ThangKetThucVu)
            {
                if (currentMonth >= sp.ThangBatDauVu && currentMonth <= sp.ThangKetThucVu)
                    isInSeason = true;
            }
            else
            {
                if (currentMonth >= sp.ThangBatDauVu || currentMonth <= sp.ThangKetThucVu)
                    isInSeason = true;
            }

            return isInSeason ? sp.HeSoGiaTrongMua : sp.HeSoGiaTraiMua;
        }
        //tính ship theo khối lượng
        private double CalculateShipping(double km, double khoiLuongKg)
        {
            double basePrice = 0;
            double pricePerKm = 0;

            if (khoiLuongKg < 50)
            {
                basePrice = 15000;
                pricePerKm = 3000;
            }

            else if (khoiLuongKg < 500)
            {
                basePrice = 40000;
                pricePerKm = 7000;
            }

            else
            {
                basePrice = 150000;
                pricePerKm = 12000;
            }

            if (km <= 2) return basePrice;

            return basePrice + ((km - 2) * pricePerKm);
        }
    }
}
