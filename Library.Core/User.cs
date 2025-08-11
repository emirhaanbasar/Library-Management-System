namespace Library.Core
{
    public class User
    {
        public int UserId { get; set; }
        public string TC { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FaceId { get; set; } = string.Empty;
        public ICollection<BookRental> BookRentals { get; set; } = new List<BookRental>();
        public ICollection<SeatReservation> SeatReservations { get; set; } = new List<SeatReservation>();
    }
} 