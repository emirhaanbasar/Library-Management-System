using Library.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Library.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IUserService _userService;
        private readonly IBookRentalService _rentalService;
        private readonly ISeatReservationService _seatService;
        public DashboardController(IBookService bookService, IUserService userService, IBookRentalService rentalService, ISeatReservationService seatService)
        {
            _bookService = bookService;
            _userService = userService;
            _rentalService = rentalService;
            _seatService = seatService;
        }
        private string CurrentUserRole => User.FindFirstValue(ClaimTypes.Role);
        private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        public async Task<IActionResult> Index()
        {
            var books = await _bookService.GetAllBooksAsync();
            var users = await _userService.GetAllUsersAsync();
            var rentals = await _rentalService.GetAllRentalsAsync();
            var reservations = await _seatService.GetActiveReservationsAsync();
            var overdue = rentals.Where(r => r.ReturnDate == null && r.DueDate < DateTime.Now).ToList();
            var model = new
            {
                BookCount = books.Count(),
                UserCount = users.Count(),
                RentalCount = rentals.Count(),
                ReservationCount = reservations.Count(),
                OverdueCount = overdue.Count(),
                MyActiveRentals = rentals.Where(r => r.UserId == CurrentUserId && r.ReturnDate == null).Count(),
                MyActiveReservations = reservations.Where(r => r.UserId == CurrentUserId).Count(),
                Role = CurrentUserRole
            };
            if (CurrentUserRole == "Student")
            {
                return View("~/Views/Student/Dashboard.cshtml", model);
            }
            return View(model);
        }
    }
} 