namespace Library.Core
{
    public class Seat
    {
        public int SeatId { get; set; }
        public int SeatNumber { get; set; }
        public ICollection<SeatReservation> SeatReservations { get; set; } = new List<SeatReservation>();
    }
} 