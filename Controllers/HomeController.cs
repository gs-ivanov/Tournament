namespace Tournament.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Diagnostics;
    using Tournament.Models;

    public class HomeController : Controller
    {
        public IActionResult Index()
        {

            return RedirectToAction("index", "teams");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
