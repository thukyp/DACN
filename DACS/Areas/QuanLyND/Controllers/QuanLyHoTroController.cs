// File: Areas/QuanLyND/Controllers/QuanLyHoTroController.cs
using System;
using System.Net.Mail;
using DACS.Models;
using DACS.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;


namespace DACS.Areas.QuanLyND.Controllers
{
    [Area("QuanLyND")] // Phải có
    [Authorize(Roles = SD.Role_Owner + "," + SD.Role_QuanLyND)]
    [Route("QuanLyND/QuanLyHoTro")]
    public class QuanLyHoTroController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IHubContext<ChatHub> _hubContext;
        public QuanLyHoTroController(ApplicationDbContext context, IHubContext<ChatHub> hubContext, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hubContext = hubContext;
            _hostEnvironment = hostEnvironment;
        }

        // --- SỬA HÀM NÀY ---
        public IActionResult Index()
        {
            // XÓA HẾT CODE CŨ LẤY TỪ DATABASE
            // var danhSach = _context.ChiTietLienHe... (XÓA DÒNG NÀY)

            // CHỈ CẦN DÒNG NÀY:
            return View();
        }
        [HttpPost("SaveMessage")]
        public async Task<IActionResult> SaveMessage([FromBody] ChatMessage message)
        {
            if (message == null || string.IsNullOrEmpty(message.Message))
                return BadRequest("Nội dung trống");

            try
            {
                message.SentTime = DateTime.Now;

                _context.ChatMessages.Add(message);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Đã lưu tin nhắn" });
            }
            catch (Exception ex)
            {
                // Ghi log lỗi để xem thực tế bị gì
                Console.WriteLine("🔥 Lỗi khi lưu tin nhắn: " + ex.Message);
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }



        [HttpGet("clients")]
        public IActionResult GetClients()
        {
            var clients = _context.ChatMessages
                .GroupBy(c => c.SenderId)
                .Select(g => new { id = g.Key, name = g.First().SenderName })
                .ToList();
            return Ok(clients);
        }

        [HttpGet("messages/{userId}")]
        public IActionResult GetMessages(string userId)
        {
            var messages = _context.ChatMessages
              .Where(c => c.SenderId == userId || c.ReceiverId == userId)
              .OrderBy(c => c.SentTime)
              .Select(c => new
              {
                  receiver = c.ReceiverId,
                  senderRole = c.IsFromAdmin ? "admin" : "user",
                  message = c.Message,
                  sentTime = c.SentTime.ToString("yyyy-MM-ddTHH:mm:ss")
              })
              .ToList();

            return Ok(messages);
        }


        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetChatHistory(string userId)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var data = await _context.ChatMessages
                .Where(x => x.SenderId == userId || x.ReceiverId == userId)
                .OrderBy(x => x.SentTime)
                .Select(x => new {
                    x.SenderId,
                    x.ReceiverId,
                    x.Message,
                    x.IsFromAdmin,
                    SentTime = x.SentTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    ImageUrl = string.IsNullOrEmpty(x.ImageUrl)
                        ? null
                        : (x.ImageUrl.StartsWith("http")
                            ? x.ImageUrl
                            : $"{baseUrl}{x.ImageUrl}")
                })
                .ToListAsync();

            return Json(data);
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file, string senderId, string receiverId)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Không có file nào được gửi lên.");


                var uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "uploads", "chat");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadPath, fileName);
                var imageUrl = $"/uploads/chat/{fileName}";

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var message = new ChatMessage
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    SenderName = senderId,
                    Message = "",
                    ImageUrl = imageUrl,
                    IsFromAdmin = true,
                    SentTime = DateTime.Now
                };

                _context.ChatMessages.Add(message);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "Upload thành công",
                    imageUrl = imageUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }



    }

}

