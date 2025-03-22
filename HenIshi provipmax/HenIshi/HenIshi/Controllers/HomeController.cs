using System.Diagnostics;
using HenIshi.Models;
using Microsoft.AspNetCore.Mvc;

namespace HenIshi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult introduce()
        {
            return View();
        }
        public IActionResult contact()
        {
            return View();
        }
        public IActionResult bocrangsu()
        {
            return View();
        }
        public IActionResult cayghepimpact()
        {
            return View();
        }
        public IActionResult niengrang()
        {
            return View();
        }
        public IActionResult matdansu()
        {
            return View();
        }
        public IActionResult taytrang()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
    }
}
