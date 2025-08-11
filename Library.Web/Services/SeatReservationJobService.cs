using Library.Service;
using Library.Web.Services;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Library.Web.Hubs;
using Library.Core;

namespace Library.Web.Services
{
    public class SeatReservationJobService
    {
        private readonly ISeatReservationService _reservationService;
        private readonly SeatReservationNotificationService _notificationService;
        private readonly IHubContext<SeatHub> _seatHub;
        private readonly IRepository<Seat> _seatRepository;

        public SeatReservationJobService(
            ISeatReservationService reservationService,
            SeatReservationNotificationService notificationService,
            IHubContext<SeatHub> seatHub,
            IRepository<Seat> seatRepository)
        {
            _reservationService = reservationService;
            _notificationService = notificationService;
            _seatHub = seatHub;
            _seatRepository = seatRepository;
        }

        // Rezervasyon oluşturulduğunda onay maili gönder
        public async Task SendReservationConfirmationJob(int reservationId)
        {
            try
            {
                Console.WriteLine($"Rezervasyon onay maili job'ı başlatıldı: {reservationId}");
                await _notificationService.SendReservationConfirmationAsync(reservationId);
                Console.WriteLine($"Rezervasyon onay maili başarıyla gönderildi: {reservationId}");
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Rezervasyon onay maili gönderilemedi: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        // Rezervasyon bitişinde hatırlatma maili gönder (15 dakika önce)
        public async Task SendReservationReminderJob(int reservationId)
        {
            try
            {
                await _notificationService.SendReservationReminderAsync(reservationId);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Rezervasyon hatırlatma maili gönderilemedi: {ex.Message}");
            }
        }

        // Rezervasyon süresi dolduğunda sona erdir ve bildirim gönder
        public async Task ExpireReservationJob(int reservationId, int seatNumber)
        {
            try
            {
                // Rezervasyonu sona erdir
                await _reservationService.ReleaseSeatAsync(reservationId);
                
                // Süre dolma bildirimi gönder
                await _notificationService.SendReservationExpirationNotificationAsync(reservationId);
                
                // SignalR ile real-time güncelleme
                await _seatHub.Clients.All.SendAsync("SeatChanged");
                
                Console.WriteLine($"Rezervasyon {reservationId} süresi doldu ve sona erdirildi.");
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Rezervasyon sona erdirme işlemi başarısız: {ex.Message}");
            }
        }

        // Rezervasyon oluşturulduğunda tüm job'ları planla
        public void ScheduleReservationJobs(int reservationId, DateTime endTime, int seatNumber)
        {
            var now = DateTime.Now;
            var reminderTime = endTime.AddMinutes(-15); // 15 dakika önce hatırlatma
            var expirationTime = endTime; // Bitiş zamanında sona erdir

            Console.WriteLine($"Rezervasyon job'ları planlanıyor: ReservationId={reservationId}, EndTime={endTime}, SeatNumber={seatNumber}");

            // Onay maili artık direkt gönderiliyor, burada sadece hatırlatma ve sona erdirme planlanıyor

            // Hatırlatma maili planla (eğer 15 dakikadan fazla süre varsa)
            if (reminderTime > now)
            {
                var reminderJobId = BackgroundJob.Schedule(
                    () => SendReservationReminderJob(reservationId),
                    reminderTime - now
                );
                Console.WriteLine($"Hatırlatma maili job'ı planlandı: JobId={reminderJobId}, ReservationId={reservationId}, ReminderTime={reminderTime}");
            }

            // Rezervasyon sona erdirme job'ı planla
            var expirationJobId = BackgroundJob.Schedule(
                () => ExpireReservationJob(reservationId, seatNumber),
                expirationTime - now
            );
            Console.WriteLine($"Rezervasyon sona erdirme job'ı planlandı: JobId={expirationJobId}, ReservationId={reservationId}, ExpirationTime={expirationTime}");
        }
    }
} 