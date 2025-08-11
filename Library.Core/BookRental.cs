using System.ComponentModel.DataAnnotations;

namespace Library.Core
{
    public class BookRental
    {
        [Key]
        public int RentalId { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public DateTime DueDate { get; set; }
    }
} 