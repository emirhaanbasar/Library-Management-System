using Library.Service;
using Library.Web.Services;
using System.Threading.Tasks;

namespace Library.Web.Services
{
    public class OverdueNotificationJob
    {
        private readonly IBookRentalService _rentalService;
        private readonly IUserService _userService;
        private readonly MailService _mailService;
        public OverdueNotificationJob(IBookRentalService rentalService, IUserService userService, MailService mailService)
        {
            _rentalService = rentalService;
            _userService = userService;
            _mailService = mailService;
        }
        public async Task SendOverdueNotificationsAsync()
        {
            var rentals = await _rentalService.GetAllRentalsAsync();
            var overdue = rentals.Where(r => r.ReturnDate == null && r.DueDate < DateTime.Now);
            foreach (var rental in overdue)
            {
                var user = await _userService.GetUserByIdAsync(rental.UserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    await _mailService.SendMailAsync(user.Email, "Geciken Kitap İadesi", $"Lütfen kiraladığınız kitabı en kısa sürede iade ediniz. Kiralama ID: {rental.RentalId}");
                }
            }
        }
    }
} 