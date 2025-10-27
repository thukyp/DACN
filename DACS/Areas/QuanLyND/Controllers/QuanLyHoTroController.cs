// File: Areas/QuanLyND/Controllers/QuanLyHoTroController.cs
using System.Net.Mail;
using DACS.Models;
using DACS.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DACS.Areas.QuanLyND.Controllers
{
    [Area("QuanLyND")] // Phải có
    [Authorize(Roles = SD.Role_Owner + "," + SD.Role_QuanLyND)]
    public class QuanLyHoTroController : Controller
    {
        private readonly ApplicationDbContext _context;
        public QuanLyHoTroController(ApplicationDbContext context)
        {
            _context = context;

        }

        // --- SỬA HÀM NÀY ---
        public IActionResult Index()
        {
            // XÓA HẾT CODE CŨ LẤY TỪ DATABASE
            // var danhSach = _context.ChiTietLienHe... (XÓA DÒNG NÀY)

            // CHỈ CẦN DÒNG NÀY:
            return View();
        }
        // --- KẾT THÚC SỬA ---
    }
}