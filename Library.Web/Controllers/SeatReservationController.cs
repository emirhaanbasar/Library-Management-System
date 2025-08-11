using Library.Core;
using Library.Service;
using Library.Web.Models;
using Library.Web.Services;
using Library.Web.Hubs;
using Microsoft.AspNetCore.Mvc;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Library.Web.Controllers
{
    [Authorize]
    public class SeatReservationController : Controller
    {
        private readonly ISeatReservationService _reservationService;
        private readonly IRepository<Seat> _seatRepository;
        private readonly SeatReservationJobService _jobService;
        private readonly IHubContext<SeatHub> _seatHub;
        private readonly IUserService _userService;
        // Not: Gerçek uygulamada kullanıcı kimliği oturumdan alınır
        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        private string CurrentUserRole => User.FindFirstValue(ClaimTypes.Role);

        public SeatReservationController(
            ISeatReservationService reservationService,
            IRepository<Seat> seatRepository,
            SeatReservationJobService jobService,
            IHubContext<SeatHub> seatHub,
            IUserService userService)
        {
            _reservationService = reservationService;
            _seatRepository = seatRepository;
            _jobService = jobService;
            _seatHub = seatHub;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            var seats = (await _seatRepository.GetAllAsync()).OrderBy(s => s.SeatNumber).ToList();
            var reservations = (await _reservationService.GetActiveReservationsAsync()).ToList();
            var model = new List<SeatReservationViewModel>();
            foreach (var seat in seats)
            {
                var active = reservations.FirstOrDefault(r => r.SeatId == seat.SeatId);
                model.Add(new SeatReservationViewModel
                {
                    SeatId = seat.SeatId,
                    SeatNumber = seat.SeatNumber,
                    IsOccupied = active != null,
                    StartTime = active?.ReservationDate.Add(active.StartTime),
                    EndTime = active?.ReservationDate.Add(active.EndTime)
                });
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Reserve(int id)
        {
            var seat = await _seatRepository.GetByIdAsync(id);
            if (seat == null) return NotFound();
            var model = new SeatReservationViewModel
            {
                SeatId = seat.SeatId,
                SeatNumber = seat.SeatNumber,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1)
            };
            return View(model);
        }

        // Eski Hangfire job metodu - artık kullanılmıyor
        public async Task ReleaseSeatJob(int seatId, int seatNumber)
        {
            await _reservationService.ReleaseSeatAsync(seatId);
            await _seatHub.Clients.All.SendAsync("SeatChanged");
        }

        [HttpPost]
        public async Task<IActionResult> Reserve(SeatReservationViewModel model)
        {
            try
            {
                if (model.StartTime == null || model.EndTime == null)
                    throw new Exception("Başlangıç ve bitiş zamanı zorunlu.");
                
                // Rezervasyon oluştur ve ID'yi al
                var reservationId = await _reservationService.ReserveSeatAsync(model.SeatId, CurrentUserId, model.StartTime.Value, model.EndTime.Value);
                Console.WriteLine($"Rezervasyon oluşturuldu: ReservationId={reservationId}, SeatId={model.SeatId}, UserId={CurrentUserId}");
                
                // Direkt mail gönder (Hangfire job'ı beklemeden)
                try
                {
                    await _jobService.SendReservationConfirmationJob(reservationId);
                    Console.WriteLine($"Rezervasyon onay maili direkt gönderildi: {reservationId}");
                }
                catch (Exception mailEx)
                {
                    Console.WriteLine($"Rezervasyon maili gönderilemedi: {mailEx.Message}");
                }
                
                // Job'ları planla (hatırlatma, süre dolma)
                _jobService.ScheduleReservationJobs(reservationId, model.EndTime.Value, model.SeatNumber);
                Console.WriteLine($"Rezervasyon job'ları planlandı: ReservationId={reservationId}");
                
                await _seatHub.Clients.All.SendAsync("SeatChanged");
                return RedirectToAction("MyReservations");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var seat = await _seatRepository.GetByIdAsync(model.SeatId);
                model.SeatNumber = seat?.SeatNumber ?? 0;
                return View(model);
            }
        }

        public async Task<IActionResult> MyReservations()
        {
            var reservations = await _reservationService.GetActiveReservationsByUserAsync(CurrentUserId);
            return View(reservations);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int reservationId)
        {
            await _reservationService.ReleaseSeatAsync(reservationId);
            await _seatHub.Clients.All.SendAsync("SeatChanged");
            return RedirectToAction("MyReservations");
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var seats = (await _seatRepository.GetAllAsync()).OrderBy(s => s.SeatNumber).ToList();
            var reservations = (await _reservationService.GetActiveReservationsAsync()).ToList();
            var users = reservations.Select(r => r.UserId).Distinct().ToList();
            var userDict = new Dictionary<int, User>();
            foreach (var userId in users)
            {
                var user = await _userService.GetUserByIdAsync(userId);
                if (user != null) userDict[userId] = user;
            }
            var model = seats.Select(seat => {
                var active = reservations.FirstOrDefault(r => r.SeatId == seat.SeatId);
                User? user = null;
                if (active != null && userDict.ContainsKey(active.UserId))
                    user = userDict[active.UserId];
                return new SeatReservationManageViewModel
                {
                    SeatId = seat.SeatId,
                    SeatNumber = seat.SeatNumber,
                    IsOccupied = active != null,
                    User = user,
                    ReservationId = active?.ReservationId ?? 0
                };
            }).ToList();
            return View(model);
        }
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        public async Task<IActionResult> RemoveUserFromSeat(int reservationId)
        {
            await _reservationService.ReleaseSeatAsync(reservationId);
            await _seatHub.Clients.All.SendAsync("SeatChanged");
            return RedirectToAction("Manage");
        }
    }
} 