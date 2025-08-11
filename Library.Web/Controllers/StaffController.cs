using Library.Core;
using Library.Service;
using Microsoft.AspNetCore.Mvc;

namespace Library.Web.Controllers
{
    public class StaffController : Controller
    {
        private readonly IUserService _userService;
        public StaffController(IUserService userService)
        {
            _userService = userService;
        }
        // Artık ayrı Dashboard veya AddStaff yok. Tüm işlemler Dashboard/Index üzerinden yapılacak.
    }
} 