using System.Net.Mail;
using DACS.Models;
using DACS.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DACS.Areas.QuanLyND.Controllers
{
    public class QuanLyHoTroController : Controller
    {
        private readonly ApplicationDbContext _context;
        public QuanLyHoTroController(ApplicationDbContext context)
        {
            _context = context;

        }
        [Area("QuanLyND")]
        [Authorize(Roles = SD.Role_Owner + "," + SD.Role_QuanLyND)] // Thêm phân quyền
        public IActionResult Index()
        {
            var danhSach = _context.ChiTietLienHe
        .OrderByDescending(x => x.NgayGui)
        .ToList();

            return View(danhSach);
        }


    }
}
