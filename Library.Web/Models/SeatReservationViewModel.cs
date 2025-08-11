using Library.Core;
using System;
using System.Collections.Generic;

namespace Library.Web.Models
{
    public class SeatReservationViewModel
    {
        public int SeatId { get; set; }
        public int SeatNumber { get; set; }
        public bool IsOccupied { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<Seat>? AllSeats { get; set; }
        public List<SeatReservation>? ActiveReservations { get; set; }
    }
} 