namespace Library.Core
{
    public interface ISeatReservationRepository : IRepository<SeatReservation>
    {
        Task<IEnumerable<SeatReservation>> GetActiveReservationsAsync();
        Task<IEnumerable<SeatReservation>> GetActiveReservationsByUserAsync(int userId);
    }
} 