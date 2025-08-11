using Library.Core;

namespace Library.Web.Models
{
    public class SeatReservationManageViewModel
    {
        public int SeatId { get; set; }
        public int SeatNumber { get; set; }
        public bool IsOccupied { get; set; }
        public User? User { get; set; }
        public int ReservationId { get; set; }
    }
} 