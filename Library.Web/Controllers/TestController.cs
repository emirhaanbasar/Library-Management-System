using Library.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Library.Web.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
        private readonly MailService _mailService;
        private readonly SeatReservationNotificationService _notificationService;

        public TestController(MailService mailService, SeatReservationNotificationService notificationService)
        {
            _mailService = mailService;
            _notificationService = notificationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TestMail(string email)
        {
            try
            {
                Console.WriteLine($"Test mail gönderiliyor: {email}");
                await _mailService.SendMailAsync(email, "Test Mail - Kütüphane Sistemi", 
                    "<h2>Test Mail</h2><p>Bu bir test mailidir. Mail sistemi çalışıyor!</p>");
                TempData["Message"] = "Test mail başarıyla gönderildi! Console loglarını kontrol edin.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test mail hatası: {ex.Message}");
                TempData["Error"] = $"Mail gönderilemedi: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> TestReservationMail(int reservationId)
        {
            try
            {
                await _notificationService.SendReservationConfirmationAsync(reservationId);
                TempData["Message"] = "Rezervasyon onay maili gönderildi!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Rezervasyon maili gönderilemedi: {ex.Message}";
            }
            return RedirectToAction("Index");
        }
    }
} 