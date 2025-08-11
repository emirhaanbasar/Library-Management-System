using Library.Core;
using Library.Service;
using Library.Web.Models;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Library.Web.Services;

namespace Library.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // TC ve Email benzersiz mi kontrolü
            var users = await _userService.GetAllUsersAsync();
            if (users.Any(u => u.TC == model.TC))
                ModelState.AddModelError("TC", "Bu TC ile kayıtlı kullanıcı var.");
            if (users.Any(u => u.Email == model.Email))
                ModelState.AddModelError("Email", "Bu e-posta ile kayıtlı kullanıcı var.");
            if (!ModelState.IsValid)
                return View(model);

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                TC = model.TC,
                Email = model.Email,
                Phone = model.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role,
                FaceId = string.Empty, // Boş string olarak ayarla
            };
            await _userService.AddUserAsync(user);
            // Kimlik doğrulama
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.TC),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            // Kayıt sonrası rol bazlı yönlendirme
            if (model.Role == "Staff")
                return RedirectToAction("Dashboard", "Staff");
            else
                return RedirectToAction("Dashboard", "Student");
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var users = await _userService.GetAllUsersAsync();
            var user = users.FirstOrDefault(u =>
                (u.TC == model.TCOrUsername || u.Username == model.TCOrUsername) &&
                BCrypt.Net.BCrypt.Verify(model.Password, u.PasswordHash));
            if (user == null)
            {
                ModelState.AddModelError("", "TC/Kullanıcı adı veya şifre hatalı.");
                return View(model);
            }
            // Kimlik doğrulama
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.TC),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            // Login sonrası rol bazlı yönlendirme
            if (user.Role == "Staff")
                return RedirectToAction("Dashboard", "Staff");
            else
                return RedirectToAction("Dashboard", "Student");
        }

        [HttpGet]
        public IActionResult StaffLogin() => View();

        [HttpPost]
        public async Task<IActionResult> StaffLogin(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var users = await _userService.GetAllUsersAsync();
            var user = users.FirstOrDefault(u =>
                u.Username == model.TCOrUsername &&
                BCrypt.Net.BCrypt.Verify(model.Password, u.PasswordHash) &&
                (u.Role == "Staff" || u.Role == "Admin"));
            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
                return View(model);
            }
            // Kimlik doğrulama
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? user.TC),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public IActionResult RegisterStaff()
        {
            if (!User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterStaff(StaffRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (string.IsNullOrWhiteSpace(model.TC))
            {
                ModelState.AddModelError("TC", "TC Kimlik No zorunludur.");
                return View(model);
            }

            var users = await _userService.GetAllUsersAsync();
            if (users.Any(u => u.Username == model.Username))
                ModelState.AddModelError("Username", "Bu kullanıcı adı ile kayıtlı personel var.");
            if (users.Any(u => u.Email == model.Email))
                ModelState.AddModelError("Email", "Bu e-posta ile kayıtlı personel var.");
            if (users.Any(u => u.TC == model.TC))
                ModelState.AddModelError("TC", "Bu TC Kimlik No ile kayıtlı personel var.");
            if (!ModelState.IsValid)
                return View(model);

            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Username = model.Username,
                Email = model.Email,
                Phone = model.Phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = "Staff",
                TC = model.TC,
                FaceId = string.Empty // Boş string olarak ayarla
            };
            await _userService.AddUserAsync(user);
            return RedirectToAction("StaffLogin");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear(); // Tüm session'ı temizle
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
} 