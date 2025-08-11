using Library.Core;

namespace Library.Service
{
    public class SeatReservationService : ISeatReservationService
    {
        private readonly ISeatReservationRepository _reservationRepository;
        private readonly IRepository<Seat> _seatRepository;
        public SeatReservationService(ISeatReservationRepository reservationRepository, IRepository<Seat> seatRepository)
        {
            _reservationRepository = reservationRepository;
            _seatRepository = seatRepository;
        }
        public async Task<IEnumerable<SeatReservation>> GetActiveReservationsAsync() => await _reservationRepository.GetActiveReservationsAsync();
        public async Task<IEnumerable<SeatReservation>> GetActiveReservationsByUserAsync(int userId) => await _reservationRepository.GetActiveReservationsByUserAsync(userId);
        public async Task<int> ReserveSeatAsync(int seatId, int userId, DateTime startTime, DateTime endTime)
        {
            // Kullanıcı aynı anda birden fazla koltuk rezerve edemez
            var userActive = await _reservationRepository.GetActiveReservationsByUserAsync(userId);
            if (userActive.Any())
                throw new Exception("Zaten aktif bir koltuk rezervasyonunuz var.");
            // Koltuk dolu mu?
            var seatActive = (await _reservationRepository.GetActiveReservationsAsync()).Any(r => r.SeatId == seatId);
            if (seatActive)
                throw new Exception("Bu koltuk şu anda dolu.");
            // Süre kontrolü (maksimum 4 saat)
            if ((endTime - startTime).TotalHours > 4)
                throw new Exception("Bir rezervasyon en fazla 4 saat olabilir.");
            // 30 dakikalık aralık kontrolü
            if ((endTime - startTime).TotalMinutes % 30 != 0)
                throw new Exception("Rezervasyon süresi 30 dakikalık aralıklarla olmalıdır.");
            var reservation = new SeatReservation
            {
                SeatId = seatId,
                UserId = userId,
                ReservationDate = startTime.Date,
                StartTime = startTime.TimeOfDay,
                EndTime = endTime.TimeOfDay
            };
            await _reservationRepository.AddAsync(reservation);
            await _reservationRepository.SaveAsync();
            return reservation.ReservationId;
        }
        public async Task ReleaseSeatAsync(int reservationId)
        {
            var reservation = await _reservationRepository.GetByIdAsync(reservationId);
            if (reservation != null)
            {
                _reservationRepository.Delete(reservation);
                await _reservationRepository.SaveAsync();
            }
        }
    }
} 