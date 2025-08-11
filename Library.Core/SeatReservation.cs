using System.ComponentModel.DataAnnotations;

namespace Library.Core
{
    public class SeatReservation
    {
        [Key]
        public int ReservationId { get; set; }
        public int SeatId { get; set; }
        public Seat? Seat { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime ReservationDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
} 