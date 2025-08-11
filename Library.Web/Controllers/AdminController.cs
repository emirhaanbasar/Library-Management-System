using Library.Core;
using Library.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using System.Threading.Tasks;

namespace Library.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        public AdminController(IUserService userService)
        {
            _userService = userService;
        }
        public async Task<IActionResult> Index()
        {
            var staff = (await _userService.GetAllUsersAsync()).Where(u => u.Role == "Staff").ToList();
            return View(staff);
        }
        [HttpGet]
        public IActionResult AddStaff()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddStaff(string firstName, string lastName, string username, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("", "Tüm alanlar zorunludur.");
                return View();
            }
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "Staff",
                FaceId = string.Empty // Boş string olarak ayarla
            };
            await _userService.AddUserAsync(user);
            return RedirectToAction("Index");
        }
        // Tüm kullanıcıları listele
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }
        // Kullanıcı detayları
        [HttpGet]
        public async Task<IActionResult> UserDetails(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }
        // Kullanıcı düzenle (GET)
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }
        // Kullanıcı düzenle (POST)
        [HttpPost]
        public async Task<IActionResult> EditUser(int id, string email, string phone, string role)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            user.Email = email;
            user.Phone = phone;
            user.Role = role;
            await _userService.UpdateUserAsync(user);
            return RedirectToAction("Users");
        }
        // Kullanıcı sil
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userService.DeleteUserAsync(id);
            return RedirectToAction("Users");
        }
    }
} 