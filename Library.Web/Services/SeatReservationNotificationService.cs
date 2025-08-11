using Library.Core;
using Library.Service;
using Library.Web.Services;
using Microsoft.Extensions.Configuration;

namespace Library.Web.Services
{
    public class SeatReservationNotificationService
    {
        private readonly ISeatReservationService _reservationService;
        private readonly IUserService _userService;
        private readonly IRepository<Seat> _seatRepository;
        private readonly MailService _mailService;
        private readonly IConfiguration _config;

        public SeatReservationNotificationService(
            ISeatReservationService reservationService,
            IUserService userService,
            IRepository<Seat> seatRepository,
            MailService mailService,
            IConfiguration config)
        {
            _reservationService = reservationService;
            _userService = userService;
            _seatRepository = seatRepository;
            _mailService = mailService;
            _config = config;
        }

        public async Task SendReservationConfirmationAsync(int reservationId)
        {
            var reservations = await _reservationService.GetActiveReservationsAsync();
            var reservation = reservations.FirstOrDefault(r => r.ReservationId == reservationId);
            
            if (reservation != null)
            {
                var user = await _userService.GetUserByIdAsync(reservation.UserId);
                var seat = await _seatRepository.GetByIdAsync(reservation.SeatId);
                
                if (user != null && seat != null && !string.IsNullOrEmpty(user.Email))
                {
                    var subject = "Koltuk Rezervasyonunuz Onaylandı";
                    var body = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <h2 style='color: #1976d2;'>Koltuk Rezervasyonunuz Onaylandı</h2>
                            <p>Sayın {user.FirstName} {user.LastName},</p>
                            <p>Koltuk rezervasyonunuz başarıyla oluşturulmuştur.</p>
                            <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                                <h3 style='margin-top: 0; color: #333;'>Rezervasyon Detayları:</h3>
                                <ul style='list-style: none; padding: 0;'>
                                    <li><strong>Koltuk Numarası:</strong> {seat.SeatNumber}</li>
                                    <li><strong>Rezervasyon Tarihi:</strong> {reservation.ReservationDate:dd.MM.yyyy}</li>
                                    <li><strong>Başlangıç Saati:</strong> {reservation.StartTime.ToString(@"hh\:mm")}</li>
                                    <li><strong>Bitiş Saati:</strong> {reservation.EndTime.ToString(@"hh\:mm")}</li>
                                    <li><strong>Rezervasyon ID:</strong> {reservation.ReservationId}</li>
                                </ul>
                            </div>
                            <p>Rezervasyonunuzun bitiş zamanında otomatik olarak sona erecektir.</p>
                            <p>İyi çalışmalar dileriz.</p>
                            <hr style='margin: 20px 0; border: none; border-top: 1px solid #ddd;'>
                            <p style='font-size: 12px; color: #666;'>Bu e-posta otomatik olarak gönderilmiştir.</p>
                        </div>";

                    await _mailService.SendMailAsync(user.Email, subject, body);
                }
            }
        }

        public async Task SendReservationExpirationNotificationAsync(int reservationId)
        {
            var reservations = await _reservationService.GetActiveReservationsAsync();
            var reservation = reservations.FirstOrDefault(r => r.ReservationId == reservationId);
            
            if (reservation != null)
            {
                var user = await _userService.GetUserByIdAsync(reservation.UserId);
                var seat = await _seatRepository.GetByIdAsync(reservation.SeatId);
                
                if (user != null && seat != null && !string.IsNullOrEmpty(user.Email))
                {
                    var subject = "Koltuk Rezervasyonunuz Sona Erdi";
                    var body = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <h2 style='color: #d32f2f;'>Koltuk Rezervasyonunuz Sona Erdi</h2>
                            <p>Sayın {user.FirstName} {user.LastName},</p>
                            <p>Koltuk rezervasyonunuz süresi dolmuştur.</p>
                            <div style='background-color: #ffebee; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #d32f2f;'>
                                <h3 style='margin-top: 0; color: #333;'>Rezervasyon Detayları:</h3>
                                <ul style='list-style: none; padding: 0;'>
                                    <li><strong>Koltuk Numarası:</strong> {seat.SeatNumber}</li>
                                    <li><strong>Rezervasyon Tarihi:</strong> {reservation.ReservationDate:dd.MM.yyyy}</li>
                                    <li><strong>Başlangıç Saati:</strong> {reservation.StartTime.ToString(@"hh\:mm")}</li>
                                    <li><strong>Bitiş Saati:</strong> {reservation.EndTime.ToString(@"hh\:mm")}</li>
                                    <li><strong>Rezervasyon ID:</strong> {reservation.ReservationId}</li>
                                </ul>
                            </div>
                            <p>Koltuk artık başka kullanıcılar tarafından rezerve edilebilir.</p>
                            <p>Yeni bir rezervasyon yapmak isterseniz kütüphane sistemini kullanabilirsiniz.</p>
                            <hr style='margin: 20px 0; border: none; border-top: 1px solid #ddd;'>
                            <p style='font-size: 12px; color: #666;'>Bu e-posta otomatik olarak gönderilmiştir.</p>
                        </div>";

                    await _mailService.SendMailAsync(user.Email, subject, body);
                }
            }
        }

        public async Task SendReservationReminderAsync(int reservationId)
        {
            var reservations = await _reservationService.GetActiveReservationsAsync();
            var reservation = reservations.FirstOrDefault(r => r.ReservationId == reservationId);
            
            if (reservation != null)
            {
                var user = await _userService.GetUserByIdAsync(reservation.UserId);
                var seat = await _seatRepository.GetByIdAsync(reservation.SeatId);
                
                if (user != null && seat != null && !string.IsNullOrEmpty(user.Email))
                {
                    var timeLeft = reservation.ReservationDate.Add(reservation.EndTime) - DateTime.Now;
                    var minutesLeft = (int)timeLeft.TotalMinutes;
                    
                    var subject = $"Koltuk Rezervasyonunuz {minutesLeft} Dakika Sonra Sona Erecek";
                    var body = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <h2 style='color: #ff9800;'>Rezervasyon Hatırlatması</h2>
                            <p>Sayın {user.FirstName} {user.LastName},</p>
                            <p>Koltuk rezervasyonunuz {minutesLeft} dakika sonra sona erecektir.</p>
                            <div style='background-color: #fff3e0; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #ff9800;'>
                                <h3 style='margin-top: 0; color: #333;'>Rezervasyon Detayları:</h3>
                                <ul style='list-style: none; padding: 0;'>
                                    <li><strong>Koltuk Numarası:</strong> {seat.SeatNumber}</li>
                                    <li><strong>Rezervasyon Tarihi:</strong> {reservation.ReservationDate:dd.MM.yyyy}</li>
                                    <li><strong>Başlangıç Saati:</strong> {reservation.StartTime.ToString(@"hh\:mm")}</li>
                                    <li><strong>Bitiş Saati:</strong> {reservation.EndTime.ToString(@"hh\:mm")}</li>
                                    <li><strong>Kalan Süre:</strong> {minutesLeft} dakika</li>
                                </ul>
                            </div>
                            <p>Rezervasyonunuz otomatik olarak sona erecektir.</p>
                            <hr style='margin: 20px 0; border: none; border-top: 1px solid #ddd;'>
                            <p style='font-size: 12px; color: #666;'>Bu e-posta otomatik olarak gönderilmiştir.</p>
                        </div>";

                    await _mailService.SendMailAsync(user.Email, subject, body);
                }
            }
        }
    }
} 