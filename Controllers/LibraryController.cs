using Microsoft.AspNetCore.Mvc;

namespace Mazzika.Controllers
{
    public class LibraryController : Controller
    {
        public IActionResult Index()
        {
            // Logic to fetch user's library
            return View();
        }
    }
}