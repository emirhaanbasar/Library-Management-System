using Library.Core;
using Microsoft.EntityFrameworkCore;

namespace Library.Data
{
    public class SeatReservationRepository : Repository<SeatReservation>, ISeatReservationRepository
    {
        private readonly LibraryDbContext _context;
        public SeatReservationRepository(LibraryDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<SeatReservation>> GetActiveReservationsAsync()
        {
            var now = DateTime.Now;
            var reservations = await _context.SeatReservations.ToListAsync();
            return reservations.Where(r => r.ReservationDate.Add(r.EndTime) > now);
        }
        public async Task<IEnumerable<SeatReservation>> GetActiveReservationsByUserAsync(int userId)
        {
            var now = DateTime.Now;
            var reservations = await _context.SeatReservations.Where(r => r.UserId == userId).ToListAsync();
            return reservations.Where(r => r.ReservationDate.Add(r.EndTime) > now);
        }
    }
} 