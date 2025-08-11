using Microsoft.AspNetCore.Mvc;

namespace Library.Web.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
} 