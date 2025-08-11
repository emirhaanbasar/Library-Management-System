using Library.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BCrypt.Net;
using System.Threading.Tasks;

namespace Library.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }
        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userService.GetUserByIdAsync(CurrentUserId);
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> Index(string email, string phone, string oldPassword, string newPassword)
        {
            var user = await _userService.GetUserByIdAsync(CurrentUserId);
            if (user == null) return NotFound();
            user.Email = email;
            user.Phone = phone;
            if (!string.IsNullOrWhiteSpace(oldPassword) && !string.IsNullOrWhiteSpace(newPassword))
            {
                if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
                {
                    ModelState.AddModelError("", "Eski şifre hatalı.");
                    return View(user);
                }
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }
            await _userService.UpdateUserAsync(user);
            ViewBag.Success = "Profil güncellendi.";
            return View(user);
        }
    }
} 