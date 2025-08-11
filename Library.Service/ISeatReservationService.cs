using Library.Core;

namespace Library.Service
{
    public interface ISeatReservationService
    {
        Task<IEnumerable<SeatReservation>> GetActiveReservationsAsync();
        Task<IEnumerable<SeatReservation>> GetActiveReservationsByUserAsync(int userId);
        Task<int> ReserveSeatAsync(int seatId, int userId, DateTime startTime, DateTime endTime);
        Task ReleaseSeatAsync(int reservationId);
    }
} 