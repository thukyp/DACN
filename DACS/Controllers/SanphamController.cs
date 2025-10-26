using DACS.Models;
using Microsoft.AspNetCore.Mvc;

namespace DACS.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SanPhamController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult TimKiem(string keyword)
        {
            var query = _context.SanPhams.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(sp => sp.TenSanPham.Contains(keyword));
            }

            var ketQua = query.ToList();
            return View("Index", ketQua);
        }
    }
}
