using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;

using System.Security.Claims;
using DACS.Areas.KhachHang.Controllers;
using DACS.Models;

using DACS.Models.ViewModels;
using DACS.Services;
using FuzzySharp;
using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace DACS.Controllers

{

    public class HomeController : Controller

    {

        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext _context; // Thay ApplicationDbContext bằng tên DbContext của bạn

        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly UserManager<ApplicationUser> _userManager; // Sử dụng ApplicationUser hoặc lớp User Identity của bạn

        private readonly IConfiguration _configuration;
        public HomeController(
            IConfiguration configuration,
      ILogger<HomeController> logger,

      ApplicationDbContext dbContext,

      IWebHostEnvironment webHostEnvironment,

      UserManager<ApplicationUser> userManager)

        {
            _configuration = configuration;

            _logger = logger;

            _context = dbContext;

            _webHostEnvironment = webHostEnvironment;

            _userManager = userManager;

        }



        public IActionResult Index()

        {

            return View();

        }

        public IActionResult Introduction()

        {

            return View();

        }

        public IActionResult Contact()
        {
            return View();
        }

        // Xử lý form liên hệ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ChiTietLienHe model)
        {
            // Kiểm tra nếu người dùng chưa đăng nhập
           


            try
            {
                model.Id = Guid.NewGuid().ToString("N").Substring(0, 10);
                model.NgayGui = DateTime.UtcNow;
                model.TrangThai = "chưa xử lý";
                _context.ChiTietLienHe.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thông tin liên hệ của bạn đã được gửi thành công.";
                return RedirectToAction("Contact");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi gửi thông tin. Vui lòng thử lại sau.";
                return View(model);
            }
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]

        public IActionResult Error()

        {

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        }

        

        public IActionResult News()

        {

            return View();

        }
        [HttpPost]
        public async Task<JsonResult> Ask([FromForm] string message)
        {
            string traLoi = "Xin lỗi, tôi chưa hiểu câu hỏi này.";

            using (var httpClient = new HttpClient())
            {
                var formData = new Dictionary<string, string> { { "message", message } };
                var content = new FormUrlEncodedContent(formData);

                // Gửi sang API Python
                var response = await httpClient.PostAsync("http://127.0.0.1:5000/predict", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                    traLoi = json.response;
                }
            }

            // Lưu lịch sử chat
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(kh => kh.UserId == userId);

            if (khachHang != null)
            {
                _context.ChatHistory.Add(new ChatHistory
                {
                    CauHoi = message,
                    CauTraLoi = traLoi,
                    NgayChat = DateTime.Now,
                    M_KhachHang = khachHang.M_KhachHang
                });
                await _context.SaveChangesAsync();
            }

            return Json(new { response = traLoi });
        }



        [HttpPost("api/chat/upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { reply = "Không có ảnh nào được gửi lên." });

            var uploadPath = Path.Combine("wwwroot/images/Uploads", file.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(uploadPath));

            using (var stream = new FileStream(uploadPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string pythonExe = @"C:\Users\hoa23\AppData\Local\Programs\Python\Python312\python.exe"; // đường dẫn Python
            string scriptPath = @"D:\Doancs(important)\2025-05-24\dacs\DACS\Services\compare_images.py"; // script so sánh
            string datasetFolder = @"D:\Doancs(important)\2025-05-24\dacs\DACS\wwwroot\images\Products";

            // Gọi Python để so sánh ảnh
            var process = new System.Diagnostics.Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = pythonExe,
                Arguments = $"\"{scriptPath}\" \"{uploadPath}\" \"{datasetFolder}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();
            string stdout = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            string firstLine = stdout.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
            string bestMatch = firstLine ?? "None";
            if (bestMatch == "None" || string.IsNullOrEmpty(bestMatch))
                return Ok(new { reply = "Không tìm thấy phụ phẩm phù hợp." });

            // Tìm mô tả phụ phẩm trong CSDL
            var matchedProduct = await _context.SanPhams
                .FirstOrDefaultAsync(p => p.AnhSanPham.Contains(bestMatch));

            string reply = matchedProduct != null
                ? $"Ảnh của bạn giống với phụ phẩm: {matchedProduct.TenSanPham}. Mô tả: {matchedProduct.MoTa}"
                : $"Ảnh của bạn giống với: {bestMatch} (chưa có mô tả trong CSDL).";

            return Ok(new { reply });
        }
        [HttpPost]
        public async Task<IActionResult> SaveMessage([FromBody] ChatMessage message)
        {
            if (message == null || string.IsNullOrEmpty(message.Message))
                return BadRequest("Nội dung trống");

            message.SentTime = DateTime.Now;

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> GetChatHistory(string userId)
        {
            var messages = await _context.ChatMessages
                .Where(x => x.SenderId == userId || x.ReceiverId == userId)
                .OrderBy(x => x.SentTime)
                .Select(x => new {
                    x.SenderId,
                    x.ReceiverId,
                    x.Message,
                    x.ImageUrl,
                    SentTime = x.SentTime.ToString("yyyy-MM-ddTHH:mm:ss")
                })
                .ToListAsync();

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = messages.Select(m => new {
                m.SenderId,
                m.ReceiverId,
                m.Message,
                ImageUrl = string.IsNullOrEmpty(m.ImageUrl) ? null : baseUrl + m.ImageUrl,
                m.SentTime
            });


            return Json(result);
        }



        [HttpPost]
        public async Task<IActionResult> UploadChatImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("Không có ảnh nào được gửi lên.");

            // 🗂️ Lưu vào thư mục wwwroot/uploads/chat/
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chat");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var url = $"/uploads/chat/{fileName}";
            return Json(new { url });
        }

    }

}